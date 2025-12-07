using MediatR;
using Meetlr.Domain.Enums;

namespace Meetlr.Module.Board.Application.Commands.UpdateTask;

public record UpdateTaskCommand : IRequest<UpdateTaskCommandResponse>
{
    public Guid BoardId { get; init; }
    public Guid Id { get; init; }
    public string? Title { get; init; }
    public string? Description { get; init; }
    public DateTime? DueDate { get; init; }
    public TaskPriority? Priority { get; init; }
    public IEnumerable<Guid>? LabelIds { get; init; }
}
