
namespace FIXIT.Domain.Entities;

public class ProviderRates
{
    public int Id { get; set; }
    public string ProviderId { get; set; }
    public Rate Rate { get; set; }
    public string Comment { get; set; }
    public bool IsDeleted { get; set; } = false;
    public ServiceProvider? Provider { get; set; }
}
