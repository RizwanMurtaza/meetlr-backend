using MediatR;
using Meetlr.Application.Common.Interfaces;
using Meetlr.Application.Interfaces;
using Meetlr.Domain.Exceptions.DomainErrors.ExceptionByArea;
using Microsoft.EntityFrameworkCore;

namespace Meetlr.Module.Board.Application.Queries.GetBoardById;

public class GetBoardByIdQueryHandler : IRequestHandler<GetBoardByIdQuery, GetBoardByIdQueryResponse>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;

    public GetBoardByIdQueryHandler(
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUserService)
    {
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
    }

    public async Task<GetBoardByIdQueryResponse> Handle(GetBoardByIdQuery request, CancellationToken cancellationToken)
    {
        var userId = _currentUserService.UserId ?? throw AuthenticationErrors.UserNotAuthenticated();

        var board = await _unitOfWork.Repository<Domain.Entities.Board>().GetQueryable()
            .Where(b => b.Id == request.Id && b.UserId == userId && !b.IsDeleted)
            .Select(b => new BoardDetailDto
            {
                Id = b.Id,
                Name = b.Name,
                Description = b.Description,
                Color = b.Color,
                Position = b.Position,
                CreatedAt = b.CreatedAt,
                UpdatedAt = b.UpdatedAt,
                Columns = b.Columns
                    .Where(c => !c.IsDeleted)
                    .OrderBy(c => c.Position)
                    .Select(c => new ColumnDto
                    {
                        Id = c.Id,
                        Name = c.Name,
                        Position = c.Position,
                        Tasks = c.Tasks
                            .Where(t => !t.IsDeleted)
                            .OrderBy(t => t.Position)
                            .Select(t => new TaskDto
                            {
                                Id = t.Id,
                                ColumnId = t.ColumnId,
                                Title = t.Title,
                                Description = t.Description,
                                DueDate = t.DueDate,
                                Position = t.Position,
                                Priority = t.Priority,
                                CreatedAt = t.CreatedAt,
                                UpdatedAt = t.UpdatedAt,
                                LabelIds = t.Labels.Select(l => l.Id).ToList()
                            }).ToList()
                    }).ToList(),
                Labels = b.Labels
                    .Where(l => !l.IsDeleted)
                    .Select(l => new LabelDto
                    {
                        Id = l.Id,
                        Name = l.Name,
                        Color = l.Color
                    }).ToList()
            })
            .FirstOrDefaultAsync(cancellationToken);

        if (board == null)
        {
            throw BoardErrors.BoardNotFound(request.Id);
        }

        return new GetBoardByIdQueryResponse
        {
            Board = board
        };
    }
}
