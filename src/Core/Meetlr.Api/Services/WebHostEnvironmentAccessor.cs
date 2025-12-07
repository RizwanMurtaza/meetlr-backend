using Meetlr.Infrastructure.Services.FileStorage;
using Microsoft.AspNetCore.Hosting;

namespace Meetlr.Api.Services;

/// <summary>
/// Provides access to IWebHostEnvironment properties for the Infrastructure layer.
/// This abstraction allows the Infrastructure layer to access web host paths without
/// depending on ASP.NET Core hosting abstractions directly.
/// </summary>
public class WebHostEnvironmentAccessor : IWebHostEnvironmentAccessor
{
    private readonly IWebHostEnvironment _environment;

    public WebHostEnvironmentAccessor(IWebHostEnvironment environment)
    {
        _environment = environment;
    }

    public string WebRootPath => _environment.WebRootPath;
    public string ContentRootPath => _environment.ContentRootPath;
}
