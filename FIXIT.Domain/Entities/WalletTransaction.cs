namespace FIXIT.Domain.Entities;

public enum TransactionType { Depit = 1, Credit = 2 }
public class WalletTransaction
{
    public int Id { get; set; }
    public int WalletId { get; set; }
    public Wallet? Wallet { get; set; }
    public Price Amount { get; set; }
    public DateTime TransactionDate { get; set; } = EgyptTimeHelper.Now;
    public int OrderId { get; set; }
    public Order? Order { get; set; }
    public TransactionType Type { get; set; }
}
