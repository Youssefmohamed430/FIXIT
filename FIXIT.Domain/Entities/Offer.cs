namespace FIXIT.Domain.Entities;

public enum OfferStatus {Pending = 1, Accepted = 2, Rejected = 3 }
public class Offer
{
    public int Id { get; set; }
    public Price Price { get; private set; }
    public string Description { get; set; }
    public DateTime CreatedAt { get; set; } = EgyptTimeHelper.Now;
    public OfferStatus status { get; set; } = OfferStatus.Pending;
    public int JobPostId { get; set; }
    public string ProviderId { get; set; }
    public ServiceProvider? ServiceProvider { get; set; }
    public JobPost? JobPost { get; set; }
    public Order? order { get; set; }

}
