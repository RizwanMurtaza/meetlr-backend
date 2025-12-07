using MediatR;
using Meetlr.Application.Common.Interfaces;
using Meetlr.Application.Interfaces;
using Meetlr.Domain.Entities.Emails;
using Meetlr.Domain.Exceptions.Http;
using Microsoft.EntityFrameworkCore;

namespace Meetlr.Application.Features.SmtpConfiguration.Commands.DeleteSmtpConfiguration;

public class DeleteSmtpConfigurationCommandHandler : IRequestHandler<DeleteSmtpConfigurationCommand, DeleteSmtpConfigurationCommandResponse>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;

    public DeleteSmtpConfigurationCommandHandler(
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUserService)
    {
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
    }

    public async Task<DeleteSmtpConfigurationCommandResponse> Handle(DeleteSmtpConfigurationCommand request, CancellationToken cancellationToken)
    {
        var config = await _unitOfWork.Repository<EmailConfiguration>()
            .GetQueryable()
            .FirstOrDefaultAsync(c => c.Id == request.Id && !c.IsDeleted, cancellationToken);

        if (config == null)
        {
            throw NotFoundException.ForEntity("SmtpConfiguration", request.Id);
        }

        // Prevent deleting system default configurations
        if (config.IsSystemDefault)
        {
            throw ValidationException.InvalidInput("SmtpConfiguration", "System default configurations cannot be deleted");
        }

        // Soft delete
        config.IsDeleted = true;
        config.DeletedAt = DateTime.UtcNow;
        config.DeletedBy = _currentUserService.UserId?.ToString();

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return new DeleteSmtpConfigurationCommandResponse
        {
            Success = true,
            Message = "SMTP configuration deleted successfully. Will revert to parent level configuration."
        };
    }
}
