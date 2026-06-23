
namespace FIXIT.Application.DTOs;

public class EscrowOrderDTO
{
    public Order? order { get; set; }
    public string? CustomerName { get; set; }
    public string? ProviderName { get; set; }
    public Price? CustomerWalletBalance { get; set; }
    public Price? ProviderWalletBalance { get; set; }
    public int CustomerWalletId { get; set; }
    public int ProviderWalletId { get; set; }
}
