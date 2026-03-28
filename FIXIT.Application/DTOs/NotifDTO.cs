namespace FIXIT.Application.DTOs;

public class NotifDTO
{
    public int NotifId { get; set; }
    public required string Message { get; set; }
    public DateTime Date { get; set; } = EgyptTimeHelper.Now;
    public required string UserId { get; set; }
    public string? UserName { get; set; }
    public bool IsRead { get; set; } = false;
}
