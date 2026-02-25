namespace FIXIT.Application.DTOs;

public class OfferDTO
{
    public int Id { get; set; }
    public Price? Price { get; private set; }
    public string? Description { get; set; }
    public DateTime CreatedAt { get; set; } = EgyptTimeHelper.Now;
    public OfferStatus status { get; set; } = OfferStatus.Pending;
    public int JobPostId { get; set; }
    public string? ProviderId { get; set; }
    public string? ProviderName { get; set; }
}
