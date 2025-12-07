using MediatR;
using Meetlr.Application.Common.Interfaces;
using Meetlr.Application.Interfaces;
using Meetlr.Domain.Exceptions.DomainErrors.ExceptionByArea;
using Microsoft.EntityFrameworkCore;

namespace Meetlr.Module.Board.Application.Queries.GetAllBoards;

public class GetAllBoardsQueryHandler : IRequestHandler<GetAllBoardsQuery, GetAllBoardsQueryResponse>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;

    public GetAllBoardsQueryHandler(
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUserService)
    {
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
    }

    public async Task<GetAllBoardsQueryResponse> Handle(GetAllBoardsQuery request, CancellationToken cancellationToken)
    {
        var userId = _currentUserService.UserId ?? throw AuthenticationErrors.UserNotAuthenticated();

        var query = _unitOfWork.Repository<Domain.Entities.Board>().GetQueryable()
            .Where(b => b.UserId == userId && !b.IsDeleted);

        // Apply search filter
        if (!string.IsNullOrWhiteSpace(request.SearchTerm))
        {
            var searchLower = request.SearchTerm.ToLower();
            query = query.Where(b =>
                b.Name.ToLower().Contains(searchLower) ||
                (b.Description != null && b.Description.ToLower().Contains(searchLower)));
        }

        // Apply sorting
        query = request.SortBy?.ToLower() switch
        {
            "name" => request.SortDescending
                ? query.OrderByDescending(b => b.Name)
                : query.OrderBy(b => b.Name),
            "createdat" => request.SortDescending
                ? query.OrderByDescending(b => b.CreatedAt)
                : query.OrderBy(b => b.CreatedAt),
            "position" or _ => request.SortDescending
                ? query.OrderByDescending(b => b.Position)
                : query.OrderBy(b => b.Position)
        };

        // Get total count before pagination
        var totalCount = await query.CountAsync(cancellationToken);

        // Apply pagination
        var boards = await query
            .Skip((request.PageNumber - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(b => new BoardDto
            {
                Id = b.Id,
                Name = b.Name,
                Description = b.Description,
                Color = b.Color,
                Position = b.Position,
                ColumnCount = b.Columns.Count(c => !c.IsDeleted),
                TaskCount = b.Columns.SelectMany(c => c.Tasks).Count(t => !t.IsDeleted),
                CreatedAt = b.CreatedAt,
                UpdatedAt = b.UpdatedAt
            })
            .ToListAsync(cancellationToken);

        var totalPages = (int)Math.Ceiling(totalCount / (double)request.PageSize);

        return new GetAllBoardsQueryResponse
        {
            Boards = boards,
            TotalCount = totalCount,
            PageNumber = request.PageNumber,
            PageSize = request.PageSize,
            TotalPages = totalPages,
            HasPreviousPage = request.PageNumber > 1,
            HasNextPage = request.PageNumber < totalPages
        };
    }
}
