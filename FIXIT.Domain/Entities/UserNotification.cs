namespace FIXIT.Domain.Entities;

public class UserNotification
{
    public required int NotifId { get; set; }
    public required string UserId { get; set; }
    public bool IsRead { get; set; } = false;
    public Notification? Notif { get; set; }
    public ApplicationUser? User { get; set; }
}
