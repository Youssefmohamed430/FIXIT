
using FIXIT.Application.DTOs;
using FIXIT.Application.IServices;
using FIXIT.Domain;
using FIXIT.Domain.Abstractions;
using FIXIT.Domain.Entities;
using FIXIT.Domain.ValueObjects;
using Mapster;

namespace FIXIT.Application.Servicces;

public class WalletService(IUnitOfWork unitOfWork) : IWallettService
{
    public async Task<Result<WalletDTO>> CreateWalletForUser(WalletDTO walletDTO)
    {
        var wallet = walletDTO.Adapt<Wallet>();

        try
        {
            await unitOfWork.GetRepository<Wallet>().AddAsync(wallet);

            await unitOfWork.SaveAsync();

            return Result<WalletDTO>.Success(walletDTO);
        }
        catch (Exception ex)
        {
            return Result<WalletDTO>.Failure(new Error("Wallet.WalletCreationFailed", ex.Message) );
        }
    }
}
