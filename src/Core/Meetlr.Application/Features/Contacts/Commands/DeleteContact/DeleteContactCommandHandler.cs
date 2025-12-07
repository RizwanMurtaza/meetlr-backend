using Meetlr.Application.Common.Interfaces;
using Meetlr.Application.Interfaces;
using Meetlr.Domain.Entities.Contacts;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Meetlr.Application.Features.Contacts.Commands.DeleteContact;

public class DeleteContactCommandHandler : IRequestHandler<DeleteContactCommand, DeleteContactCommandResponse>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;

    public DeleteContactCommandHandler(
        IUnitOfWork unitOfWork,
        ICurrentUserService _currentUserService)
    {
        _unitOfWork = unitOfWork;
        this._currentUserService = _currentUserService;
    }

    public async Task<DeleteContactCommandResponse> Handle(DeleteContactCommand request, CancellationToken cancellationToken)
    {
        var userId = _currentUserService.UserId ?? throw new UnauthorizedAccessException("User not authenticated");

        var contact = await _unitOfWork.Repository<Contact>().GetQueryable()
            .FirstOrDefaultAsync(c => c.Id == request.Id && c.UserId == userId, cancellationToken);

        if (contact == null)
        {
            throw new KeyNotFoundException($"Contact with ID {request.Id} not found");
        }

        // Soft delete
        contact.IsDeleted = true;
        contact.DeletedAt = DateTime.UtcNow;

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return new DeleteContactCommandResponse
        {
            Success = true,
            Message = "Contact deleted successfully"
        };
    }
}
