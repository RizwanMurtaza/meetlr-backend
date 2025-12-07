namespace Meetlr.Module.Board.Application.Commands.CreateBoard;

public record CreateBoardCommandResponse
{
    public Guid Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public string? Description { get; init; }
    public string Color { get; init; } = string.Empty;
    public int Position { get; init; }
    public DateTime CreatedAt { get; init; }
}
