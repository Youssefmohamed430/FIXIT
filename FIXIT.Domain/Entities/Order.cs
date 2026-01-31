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
    public Price TotalAmount { get; set; }
    public Price ProviderAmount { get; set; }
    public Price PlatformCommission { get; set; }
    public DateTime CreatedAt { get; set; } = EgyptTimeHelper.Now;
    public WorkStatus WorkStatus { get; set; } = WorkStatus.InProgress;
    public PaymentStatus PaymentStatus { get; set; } = PaymentStatus.Pending;
    public bool IsDeleted { get; set; } = false;
    public List<WalletTransaction>? walletTransactions { get; set; }
}

/*
 * Steps to create wallet transactions for an order:
 Order #123
 ├─ WalletTransaction (Customer -1000)
 ├─ WalletTransaction (Escrow +1000)
 ├─ WalletTransaction (Platform +150)
 └─ WalletTransaction (Provider +850)
 */
