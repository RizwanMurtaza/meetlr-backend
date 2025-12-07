using MediatR;
using Meetlr.Application.Common.Interfaces;
using Meetlr.Application.Interfaces;
using Meetlr.Domain.Entities.Emails;
using Meetlr.Domain.Exceptions.Http;
using Microsoft.EntityFrameworkCore;

namespace Meetlr.Application.Features.SmtpConfiguration.Commands.UpdateSmtpConfiguration;

public class UpdateSmtpConfigurationCommandHandler : IRequestHandler<UpdateSmtpConfigurationCommand, UpdateSmtpConfigurationCommandResponse>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;

    public UpdateSmtpConfigurationCommandHandler(
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUserService)
    {
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
    }

    public async Task<UpdateSmtpConfigurationCommandResponse> Handle(UpdateSmtpConfigurationCommand request, CancellationToken cancellationToken)
    {
        var config = await _unitOfWork.Repository<EmailConfiguration>()
            .GetQueryable()
            .FirstOrDefaultAsync(c => c.Id == request.Id && !c.IsDeleted, cancellationToken);

        if (config == null)
        {
            throw NotFoundException.ForEntity("SmtpConfiguration", request.Id);
        }

        // Prevent editing system default configurations
        if (config.IsSystemDefault)
        {
            throw ValidationException.InvalidInput("SmtpConfiguration", "System default configurations cannot be modified");
        }

        // Update configuration
        config.SmtpHost = request.SmtpHost;
        config.SmtpPort = request.SmtpPort;
        config.SmtpUsername = request.SmtpUsername;
        if (!string.IsNullOrEmpty(request.SmtpPassword))
        {
            config.SmtpPassword = request.SmtpPassword; // Should be encrypted in production
        }
        config.FromEmail = request.FromEmail;
        config.FromName = request.FromName;
        config.EnableSsl = request.EnableSsl;
        config.UpdatedAt = DateTime.UtcNow;
        config.UpdatedBy = _currentUserService.UserId?.ToString();

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return new UpdateSmtpConfigurationCommandResponse
        {
            Id = config.Id,
            Success = true,
            Message = "SMTP configuration updated successfully"
        };
    }
}
