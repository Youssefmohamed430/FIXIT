
namespace FIXIT.Domain.Entities;

public class ServiceProvider
{
    public required string Id { get; set; }
    public ApplicationUser? User { get; set; }
    public List<ProviderRates>? Rates { get; set; }
    public List<Offer>? Offers { get; set; }
}
