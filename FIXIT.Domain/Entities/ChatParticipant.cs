namespace FIXIT.Domain.Entities;

public class ChatParticipant
{
    public int Id { get; set; }
    public int ChatId { get; set; }
    public Chat? Chat { get; set; }
    public string UserId { get; set; }
    public ApplicationUser? User { get; set; }
}
