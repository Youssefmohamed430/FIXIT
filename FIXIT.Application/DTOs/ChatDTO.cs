namespace FIXIT.Application.DTOs;

public class ChatDTO
{
    public int ChatId { get; set; }
    public DateTime CreatedAt { get; set; } = EgyptTimeHelper.Now;
    public string? ParticipantOneId { get; set; }
    public string? ParticipantTwoId { get; set; }
    public List<MessageDto>? Messages { get; set; }
}
public class MessageDto
{
    public int Id { get; set; }
    public string SenderId { get; set; }
    public string Message { get; set; }
    public bool IsRead { get; set; }
    public DateTime SentAt { get; set; }
}