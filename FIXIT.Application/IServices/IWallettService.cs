
using Microsoft.AspNetCore.Mvc;

namespace FIXIT.Application.IServices;

public interface IWallettService
{
    Task<Result<WalletDTO>> CreateWalletForUser(WalletDTO walletDTO);
    Task<Result<WalletDTO>> GetWalletByUserId(string userId);
    Task<string> ChargeWallet(double amount, string customerid);
    Task<Result<object>> PaymobCallback(PaymobCallback payload, string hmacHeader);
    Task<Result<WalletDTO>> TransferMoney(int orderid,int WalletSenderId,int WalletRecieverId, decimal Transferedamount);
}
