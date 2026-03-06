
namespace FIXIT.Domain.Entities;

public enum WorkStatus
{ Pending = 1,Accepted = 2,InProgress = 3,CompletedByProvider = 4,Completed = 5,RevisionRequested = 6,Cancelled = 7}
public enum PaymentStatus {Pending = 1,Held = 2, Paid = 3, Failed = 4, Refunded = 5}
public class Order
{
    public int Id { get; set; }
    public int JobPostId { get; set; }
    public JobPost? JobPost { get; set; }
    public int OfferId { get; set; }
    public Offer? Offer { get; set; }
    public Price TotalAmount { get; set; }
    public Price ProviderAmount { get; set; } = Price.Create(0);
    public Price PlatformCommission { get; set; } = Price.Create(0);
    public DateTime CreatedAt { get; set; } = EgyptTimeHelper.Now;
    public WorkStatus WorkStatus { get; set; } = WorkStatus.Pending;
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
