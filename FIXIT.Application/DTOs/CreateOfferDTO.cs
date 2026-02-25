namespace FIXIT.Application.DTOs;

public class CreateOfferDTO
{
    public Price? Price { get; private set; }
    public string? Description { get; set; }
    public int JobPostId { get; set; }
    public string? ProviderId { get; set; }
}
