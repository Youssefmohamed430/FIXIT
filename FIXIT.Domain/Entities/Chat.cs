namespace FIXIT.Domain.Entities;

public class Chat
{
    public int Id { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public List<ChatParticipant>? Participants { get; set; }
    public List<ChatMessage>? Messages { get; set; }
}
