using System.Reflection;
using System.Text;
using Meetlr.Domain.Entities.Users;
using Meetlr.Module.Notifications;
using Meetlr.Plugins.Payment.Stripe;
using Meetlr.Plugins.MeetingTypes;
using Meetlr.Module.Calendar;
using Meetlr.Module.Board;
using Meetlr.Module.Homepage;
using Meetlr.Module.Analytics.Extensions;
using Meetlr.Module.SlotInvitation;
using Meetlr.Module.Billing;
using Meetlr.Api.Middleware;
using Meetlr.Api.Services;
using Meetlr.Application;
using Meetlr.Infrastructure.Services.FileStorage;
using Meetlr.Application.Common.Settings;
using Meetlr.Infrastructure;
using Meetlr.Infrastructure.Data;
using Meetlr.Scheduler;
using Meetlr.Infrastructure.Middleware;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddApplicationServices();
builder.Services.AddInfrastructureServices(builder.Configuration);
builder.Services.AddSchedulerServices();

// Register non-controller plugins
builder.Services.AddNotificationsModule(builder.Configuration);
builder.Services.AddMeetingTypesPlugin(builder.Configuration);

// Configure Zoom settings for API layer
builder.Services.Configure<Meetlr.Api.Endpoints.Communication.VideoConferencing.ZoomSettings>(
    builder.Configuration.GetSection("VideoConferencing:Zoom"));

// Configuration Settings
builder.Services.Configure<RecurringBookingsSettings>(builder.Configuration.GetSection("RecurringBookings"));
builder.Services.Configure<ApplicationUrlsSettings>(builder.Configuration.GetSection("ApplicationUrls"));
builder.Services.Configure<ExternalApisSettings>(builder.Configuration.GetSection("ExternalApis"));
builder.Services.Configure<CacheSettings>(builder.Configuration.GetSection("Cache"));

// Identity
builder.Services.AddIdentity<User, IdentityRole<Guid>>(options =>
{
    options.Password.RequireDigit = true;
    options.Password.RequireLowercase = true;
    options.Password.RequireUppercase = true;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequiredLength = 8;
    options.User.RequireUniqueEmail = true;
})
.AddEntityFrameworkStores<ApplicationDbContext>()
.AddDefaultTokenProviders();

builder.Services.AddHttpContextAccessor();

// File Storage - Register the web host environment accessor for the infrastructure layer
builder.Services.AddSingleton<IWebHostEnvironmentAccessor, WebHostEnvironmentAccessor>();

// Controllers + Plugins with controllers (each plugin registers its own controllers)
builder.Services.AddControllers()
    .AddStripePaymentProvider(builder.Configuration)
    .AddAnalyticsPlugin()
    .AddCalendarModule(builder.Configuration)
    .AddBoardModule(builder.Configuration)
    .AddHomepageModule(builder.Configuration)
    .AddSlotInvitationModule(builder.Configuration)
    .AddBillingModule(builder.Configuration);

// JWT Authentication
var jwtSettings = builder.Configuration.GetSection("JwtSettings");
var secretKey = jwtSettings["SecretKey"] ?? throw new InvalidOperationException("JWT SecretKey not configured");

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = jwtSettings["Issuer"],
        ValidAudience = jwtSettings["Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey)),
        ClockSkew = TimeSpan.Zero
    };
});

builder.Services.AddAuthorization();

// CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

// Swagger/OpenAPI
builder.Services.AddEndpointsApiExplorer();

// Add OpenAPI document generation for NSwag
builder.Services.AddOpenApiDocument(config =>
{
    config.Title = "Calendly Clone API";
    config.Version = "v1";
    config.Description = "API for Calendly Clone - Meeting Scheduling Application";

    // JWT Authentication in OpenAPI
    config.AddSecurity("Bearer", new NSwag.OpenApiSecurityScheme
    {
        Type = NSwag.OpenApiSecuritySchemeType.ApiKey,
        Name = "Authorization",
        In = NSwag.OpenApiSecurityApiKeyLocation.Header,
        Description = "JWT Authorization header using the Bearer scheme. Enter 'Bearer' [space] and then your token"
    });
});

builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Calendly Clone API",
        Version = "v1",
        Description = "API for Calendly Clone - Meeting Scheduling Application"
    });

    // JWT Authentication in Swagger
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme. Enter 'Bearer' [space] and then your token",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });

    var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    if (File.Exists(xmlPath))
    {
        c.IncludeXmlComments(xmlPath);
    }
});

var app = builder.Build();

// Initialize database with seeding
using (var scope = app.Services.CreateScope())
{
    try
    {
        await DbInitializer.InitializeAsync(scope.ServiceProvider);
    }
    catch (Exception ex)
    {
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "An error occurred while initializing the database");
        // Don't throw - allow API to start even if database initialization fails
        // This is useful for client generation when database is already initialized
    }
}

// Configure the HTTP request pipeline

// Use global exception handling middleware (must be early in the pipeline)
app.UseMiddleware<GlobalExceptionMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Calendly Clone API v1");
        c.RoutePrefix = string.Empty; // Serve Swagger UI at root
    });
}

// Only use HTTPS redirection in production
if (!app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}

app.UseCors("AllowAll");

// Serve static files from wwwroot (if exists)
app.UseStaticFiles();

// Serve uploaded files from the uploads directory
var uploadsPath = Path.Combine(app.Environment.ContentRootPath, "uploads");
if (!Directory.Exists(uploadsPath))
{
    Directory.CreateDirectory(uploadsPath);
}
app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new Microsoft.Extensions.FileProviders.PhysicalFileProvider(uploadsPath),
    RequestPath = "/uploads"
});

app.UseAuthentication();

// Tenant resolution middleware - must come after UseAuthentication so context.User is populated
app.UseTenantResolution();

app.UseAuthorization();

app.MapControllers();

app.Run();
