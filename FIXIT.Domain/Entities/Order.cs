namespace FIXIT.Domain.Entities;

public enum WorkStatus { InProgress = 1, Completed = 2}
public enum PaymentStatus {Pending = 1, Paid = 2, Failed = 3, Refunded = 4}
public class Order
{
    public int Id { get; set; }
    public int JobPostId { get; set; }
    public JobPost? JobPost { get; set; }
    public int OfferId { get; set; }
    public Offer? Offer { get; set; }
    public decimal TotalAmount { get; set; }
    public decimal ProviderAmount { get; set; }
    public decimal PlatformCommission { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public WorkStatus WorkStatus { get; set; } = WorkStatus.InProgress;
    public PaymentStatus PaymentStatus { get; set; } = PaymentStatus.Pending;
}
