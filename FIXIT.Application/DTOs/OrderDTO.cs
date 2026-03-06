
namespace FIXIT.Application.DTOs;

public class OrderDTO
{
    public int JobPostId { get; set; }
    public int OfferId { get; set; }
    public Price TotalAmount { get; set; }
    public Price ProviderAmount { get; set; } = Price.Create(0);
    public Price PlatformCommission { get; set; } = Price.Create(0);
    public DateTime CreatedAt { get; set; } = EgyptTimeHelper.Now;
    public WorkStatus WorkStatus { get; set; } = WorkStatus.InProgress;
    public PaymentStatus PaymentStatus { get; set; } = PaymentStatus.Pending;
}
