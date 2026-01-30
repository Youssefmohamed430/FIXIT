namespace FIXIT.Domain.Entities;

public enum OwnerType { Platform = 1, Customer = 2, Provider = 3}
public class Wallet
{
    public int Id { get; set; }
    public string UserId { get; set; }
    public Price Balance { get; set; }
    public OwnerType ownerType { get; set; }
    public ApplicationUser? User { get; set; }
}
