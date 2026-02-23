
namespace FIXIT.Application.IServices;

public interface IWallettService
{
    Task<Result<WalletDTO>> CreateWalletForUser(WalletDTO walletDTO);
}
