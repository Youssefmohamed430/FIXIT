namespace FIXIT.Domain.Entities;

public class Notification
{
    public int NotifId { get; set; }
    public required string Message { get; set; }
    public DateTime Date { get; set; }
    public List<UserNotification>? UserNotifications { get; set; }
}
