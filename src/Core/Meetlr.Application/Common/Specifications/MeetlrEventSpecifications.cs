using Meetlr.Domain.Entities.Events;

namespace Meetlr.Application.Common.Specifications;

/// <summary>
/// Specifications for eventType queries
/// </summary>
public static class MeetlrEventSpecifications
{
    public class ByIdWithUser : BaseSpecification<MeetlrEvent>
    {
        public ByIdWithUser(Guid meetlrEventId)
            : base(e => e.Id == meetlrEventId)
        {
            AddInclude(x => x.User);
            AddInclude(x => x.Questions);
        }
    }

    public class ByIdWithUserAndStripeAccount : BaseSpecification<MeetlrEvent>
    {
        public ByIdWithUserAndStripeAccount(Guid meetlrEventId)
            : base(e => e.Id == meetlrEventId)
        {
            AddInclude("User.StripeAccount"); // Include User and nested StripeAccount
            AddInclude(x => x.Questions);
        }
    }

    public class ActiveByUser : BaseSpecification<MeetlrEvent>
    {
        public ActiveByUser(Guid userId)
            : base(e => e.UserId == userId && e.Status == Meetlr.Domain.Enums.MeetlrEventStatus.Active)
        {
        }
    }
}
