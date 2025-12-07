namespace Meetlr.Application.Features.Availability.Queries.ValidateMinimumNotice;

public class ValidateMinimumNoticeQueryResponse
{
    public bool IsValid { get; set; }
    public string? ErrorMessage { get; set; }
}
