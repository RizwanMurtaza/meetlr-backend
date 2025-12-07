using MediatR;
using Meetlr.Application.Common.Interfaces;
using Meetlr.Domain.Entities.Emails;
using Microsoft.EntityFrameworkCore;

namespace Meetlr.Application.Features.SmtpConfiguration.Queries.GetSmtpConfigurations;

public class GetSmtpConfigurationsQueryHandler : IRequestHandler<GetSmtpConfigurationsQuery, GetSmtpConfigurationsQueryResponse>
{
    private readonly IUnitOfWork _unitOfWork;

    public GetSmtpConfigurationsQueryHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<GetSmtpConfigurationsQueryResponse> Handle(GetSmtpConfigurationsQuery request, CancellationToken cancellationToken)
    {
        var query = _unitOfWork.Repository<EmailConfiguration>()
            .GetQueryable()
            .Where(c => !c.IsDeleted);

        // Filter by level
        if (request.UserId.HasValue)
        {
            // Get user-level configurations
            query = query.Where(c => c.UserId == request.UserId);
        }
        else if (request.TenantId.HasValue)
        {
            // Get tenant-level configurations
            query = query.Where(c => c.TenantId == request.TenantId && c.UserId == null);
        }
        else
        {
            // Get system-level configurations
            query = query.Where(c => c.IsSystemDefault);
        }

        var configurations = await query.ToListAsync(cancellationToken);

        var dtos = configurations.Select(c => new SmtpConfigurationDto
        {
            Id = c.Id,
            SmtpHost = c.SmtpHost,
            SmtpPort = c.SmtpPort,
            SmtpUsername = c.SmtpUsername,
            FromEmail = c.FromEmail,
            FromName = c.FromName,
            EnableSsl = c.EnableSsl,
            IsActive = c.IsActive,
            Level = c.IsSystemDefault ? "System" : (c.UserId.HasValue ? "User" : "Tenant"),
            LastTestedAt = c.LastTestedAt,
            LastTestSucceeded = c.LastTestSucceeded,
            LastTestError = c.LastTestError
        }).ToList();

        return new GetSmtpConfigurationsQueryResponse
        {
            Configurations = dtos
        };
    }
}
