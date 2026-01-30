using Data_Access_Layer.Helpers;

namespace FIXIT.Domain.Entities;

public class ChatMessage
{
    public int Id { get; set; }
    public int ChatId { get; set; }
    public Chat? Chat { get; set; }
    public string SenderId { get; set; }
    public ApplicationUser? Sender { get; set; }
    public string Message { get; set; }
    public bool IsRead { get; set; } = false;
    public DateTime SentAt { get; set; } = EgyptTimeHelper.Now;
}
