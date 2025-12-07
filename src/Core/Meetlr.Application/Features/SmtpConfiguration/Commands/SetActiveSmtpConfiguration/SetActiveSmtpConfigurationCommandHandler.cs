using MediatR;
using Meetlr.Application.Common.Interfaces;
using Meetlr.Domain.Entities.Emails;
using Meetlr.Domain.Exceptions.Http;
using Microsoft.EntityFrameworkCore;

namespace Meetlr.Application.Features.SmtpConfiguration.Commands.SetActiveSmtpConfiguration;

public class SetActiveSmtpConfigurationCommandHandler : IRequestHandler<SetActiveSmtpConfigurationCommand, SetActiveSmtpConfigurationCommandResponse>
{
    private readonly IUnitOfWork _unitOfWork;

    public SetActiveSmtpConfigurationCommandHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<SetActiveSmtpConfigurationCommandResponse> Handle(SetActiveSmtpConfigurationCommand request, CancellationToken cancellationToken)
    {
        var config = await _unitOfWork.Repository<EmailConfiguration>()
            .GetQueryable()
            .FirstOrDefaultAsync(c => c.Id == request.Id && !c.IsDeleted, cancellationToken);

        if (config == null)
        {
            throw NotFoundException.ForEntity("SmtpConfiguration", request.Id);
        }

        // Deactivate all other configurations at the same level
        var sameLevelConfigs = await _unitOfWork.Repository<EmailConfiguration>()
            .GetQueryable()
            .Where(c =>
                c.TenantId == config.TenantId &&
                c.UserId == config.UserId &&
                c.Id != config.Id &&
                !c.IsDeleted)
            .ToListAsync(cancellationToken);

        foreach (var otherConfig in sameLevelConfigs)
        {
            otherConfig.IsActive = false;
        }

        // Activate the selected configuration
        config.IsActive = true;

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return new SetActiveSmtpConfigurationCommandResponse
        {
            Success = true,
            Message = "SMTP configuration set as active successfully"
        };
    }
}
