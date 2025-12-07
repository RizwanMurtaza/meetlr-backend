using Meetlr.Domain.Exceptions.Base;
using Meetlr.Domain.Exceptions.Http;

namespace Meetlr.Domain.Exceptions.DomainErrors.ExceptionByArea;

/// <summary>
/// Defines errors related to board/kanban management.
/// All errors use ApplicationArea.Boards (area code: 18)
/// </summary>
public static class BoardErrors
{
    private const ApplicationArea Area = ApplicationArea.Boards;

    public static NotFoundException BoardNotFound(Guid boardId, string? customMessage = null)
    {
        var exception = new NotFoundException(Area, 1, customMessage ?? "Board not found");
        exception.WithDetail("BoardId", boardId);
        return exception;
    }

    public static NotFoundException ColumnNotFound(Guid columnId, string? customMessage = null)
    {
        var exception = new NotFoundException(Area, 2, customMessage ?? "Column not found");
        exception.WithDetail("ColumnId", columnId);
        return exception;
    }

    public static NotFoundException TaskNotFound(Guid taskId, string? customMessage = null)
    {
        var exception = new NotFoundException(Area, 3, customMessage ?? "Task not found");
        exception.WithDetail("TaskId", taskId);
        return exception;
    }

    public static NotFoundException LabelNotFound(Guid labelId, string? customMessage = null)
    {
        var exception = new NotFoundException(Area, 4, customMessage ?? "Label not found");
        exception.WithDetail("LabelId", labelId);
        return exception;
    }

    public static ForbiddenException NotBoardOwner(string? customMessage = null)
        => new(Area, 5, customMessage ?? "You do not have permission to access this board");
}
