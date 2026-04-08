namespace FIXIT.Application.DTOs;

public class ChatDTO
{
    public int ChatId { get; set; }
    public DateTime CreatedAt { get; set; } = EgyptTimeHelper.Now;
    public List<ChatUserDto> Participants { get; set; } = new();
    public List<MessageDto>? Messages { get; set; }
}
public class MessageDto
{
    public int Id { get; set; }
    public int ChatId { get; set; }
    public string SenderId { get; set; }
    public string SenderName { get; set; }
    public string? SenderImage { get; set; }
    public string? RecieverId { get; set; }
    public string? RecieverName { get; set; }
    public string Message { get; set; }
    public bool IsRead { get; set; } = false;
    public DateTime SentAt { get; set; } = EgyptTimeHelper.Now;
}

public class ChatUserDto
{
    public string UserId { get; set; }
    public string Name { get; set; }
    public string? ImageUrl { get; set; }
}