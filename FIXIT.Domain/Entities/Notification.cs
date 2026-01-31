using Data_Access_Layer.Helpers;
using System.ComponentModel.DataAnnotations;

namespace FIXIT.Domain.Entities;

public class Notification
{
    [Key]
    public int NotifId { get; set; }
    public required string Message { get; set; }
    public DateTime Date { get; set; } = EgyptTimeHelper.Now;
    public List<UserNotification>? UserNotifications { get; set; }
}
