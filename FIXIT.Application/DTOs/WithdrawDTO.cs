namespace FIXIT.Application.DTOs;

public class WithdrawDTO
{
    public string MobileNumber { get; set; } = null!;
    public decimal Amount { get; set; }
    public int WalletId { get; set; }
}
