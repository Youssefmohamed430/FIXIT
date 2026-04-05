
using Microsoft.AspNetCore.Mvc;

namespace FIXIT.Application.IServices;

public interface IWallettService
{
    Task<Result<WalletDTO>> CreateWalletForUser(WalletDTO walletDTO);
    Task<Result<WalletDTO>> GetWalletByUserId(string userId);
    Task<string> ChargeWallet(double amount, string customerid,PaymentWay paymentWay);
    Task<Result<object>> RecieveCallback(object payload, string hmacHeader, PaymentWay paymentWayPa);
    Task<Result<WalletDTO>> TransferMoney(int orderid,int WalletSenderId,int WalletRecieverId, decimal Transferedamount);
    Task<Result<WalletDTO>> Withdraw(WithdrawDTO withdrawDTO);
}
