using Meetlr.Domain.Common;
using Meetlr.Domain.Entities.Users;

namespace Meetlr.Domain.Entities.Tenancy;

public class UserGroup : BaseEntity
{
    public Guid UserId { get; set; }
    public Guid GroupId { get; set; }
    public bool IsAdmin { get; set; } = false; // Is user an admin of this group
    public DateTime JoinedAt { get; set; } = DateTime.UtcNow;

    // Navigation properties
    public User User { get; set; } = null!;
    public Group Group { get; set; } = null!;
}
