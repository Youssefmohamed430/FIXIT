
namespace FIXIT.Application.IServices;

public interface IWallettService
{
    Task<Result<WalletDTO>> CreateWalletForUser(WalletDTO walletDTO);
    Task<Result<WalletDTO>> GetWalletByUserId(string userId);
    Task<Result<WalletDTO>> TransferMoney(int WalletSenderId,int WalletRecieverId, decimal Transferedamount);
}
