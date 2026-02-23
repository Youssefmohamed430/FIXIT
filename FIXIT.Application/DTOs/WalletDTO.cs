
namespace FIXIT.Application.DTOs;

public class WalletDTO
{
    public int? Id { get; set; }
    public string? Name { get; set; }
    public string? email { get; set; }
    public Price Balance { get; set; } = Price.Create(0, "EGP");
    public required string UserId { get; set; }
    public required OwnerType ownerType { get; set; }
}
