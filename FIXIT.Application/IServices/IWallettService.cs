

using FIXIT.Application.DTOs;
using FIXIT.Domain.Abstractions;

namespace FIXIT.Application.IServices;

public interface IWallettService
{
    Task<Result<WalletDTO>> CreateWalletForUser(WalletDTO walletDTO);
}
