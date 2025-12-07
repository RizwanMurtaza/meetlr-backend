using MediatR;
using Meetlr.Application.Common.Interfaces;
using Meetlr.Application.Interfaces;
using Meetlr.Domain.Entities.Emails;
using Meetlr.Domain.Exceptions.Http;
using Microsoft.EntityFrameworkCore;

namespace Meetlr.Application.Features.SmtpConfiguration.Commands.CreateSmtpConfiguration;

public class CreateSmtpConfigurationCommandHandler : IRequestHandler<CreateSmtpConfigurationCommand, CreateSmtpConfigurationCommandResponse>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;

    public CreateSmtpConfigurationCommandHandler(
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUserService)
    {
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
    }

    public async Task<CreateSmtpConfigurationCommandResponse> Handle(CreateSmtpConfigurationCommand request, CancellationToken cancellationToken)
    {
        // Check if configuration already exists at this level
        var existingConfig = await _unitOfWork.Repository<EmailConfiguration>()
            .GetQueryable()
            .FirstOrDefaultAsync(c =>
                c.TenantId == request.TenantId &&
                c.UserId == request.UserId &&
                !c.IsDeleted,
                cancellationToken);

        if (existingConfig != null)
        {
            throw ValidationException.InvalidInput("SmtpConfiguration", "A configuration already exists at this level");
        }

        var config = new EmailConfiguration
        {
            Id = Guid.NewGuid(),
            TenantId = request.TenantId,
            UserId = request.UserId,
            SmtpHost = request.SmtpHost,
            SmtpPort = request.SmtpPort,
            SmtpUsername = request.SmtpUsername,
            SmtpPassword = request.SmtpPassword, // Should be encrypted in production
            FromEmail = request.FromEmail,
            FromName = request.FromName,
            EnableSsl = request.EnableSsl,
            IsActive = false, // Not active by default
            IsSystemDefault = false,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = _currentUserService.UserId?.ToString()
        };

        _unitOfWork.Repository<EmailConfiguration>().Add(config);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return new CreateSmtpConfigurationCommandResponse
        {
            Id = config.Id,
            Success = true,
            Message = "SMTP configuration created successfully"
        };
    }
}
