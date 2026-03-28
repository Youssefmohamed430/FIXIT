
namespace FIXIT.Application.DTOs;

public class OrderDTO
{
    public int Id { get; set; }
    public int JobPostId { get; set; }
    public int OfferId { get; set; }
    public decimal TotalAmount { get; set; }
    public decimal ProviderAmount { get; set; } 
    public decimal PlatformCommission { get; set; }
    public DateTime CreatedAt { get; set; } = EgyptTimeHelper.Now;
    public WorkStatus WorkStatus { get; set; } = WorkStatus.InProgress;
    public PaymentStatus PaymentStatus { get; set; } = PaymentStatus.Pending;
}
