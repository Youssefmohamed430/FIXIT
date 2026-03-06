
using System.Security.AccessControl;
using System.Threading.Tasks;

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
    public async Task<Result<WalletDTO>> GetWalletByUserId(string userId)
    {
        var wallet = await unitOfWork.GetRepository<Wallet>().FindAsync(w => w.UserId == userId);

        if (wallet == null)
            return Result<WalletDTO>.Failure(new Error("Wallet.NotFound", "Wallet for this user not found"));
        
        return Result<WalletDTO>.Success(wallet.Adapt<WalletDTO>());
    }

    public async Task<Result<WalletDTO>> TransferMoney(int WalletSenderId, int WalletRecieverId,decimal Transferedamount)
    {
        try
        {
            unitOfWork.BeginTransaction();

            var senderWallet = await unitOfWork.GetRepository<Wallet>().FindAsync(w => w.Id == WalletSenderId);
            var recieverWallet = await unitOfWork.GetRepository<Wallet>().FindAsync(w => w.Id == WalletRecieverId);
            
            await HandleBalanceWallets(Transferedamount, senderWallet, recieverWallet);

            await unitOfWork.GetRepository<Wallet>().UpdateAsync(senderWallet);
            await unitOfWork.GetRepository<Wallet>().UpdateAsync(recieverWallet);

            await unitOfWork.SaveAsync();

            unitOfWork.Commit();
            return Result<WalletDTO>.Success(senderWallet.Adapt<WalletDTO>());
        }
        catch (Exception ex)
        {
            unitOfWork.Rollback();
            return Result<WalletDTO>.Failure(new Error("Wallet.TransferFailed", ex.Message));
        }
    }

    private async Task HandleBalanceWallets(decimal Transferedamount, Wallet senderWallet, Wallet recieverWallet)
    {
        var balance = senderWallet.Balance.Amount;
        balance -= Transferedamount;
        senderWallet.Balance = Price.Create(balance);

        var recieverBalance = recieverWallet.Balance.Amount;
        recieverBalance += Transferedamount;
        recieverWallet.Balance = Price.Create(recieverBalance);

        await CreateTransactions(Transferedamount, senderWallet, recieverWallet);
    }

    private async Task CreateTransactions(decimal Transferedamount, Wallet senderWallet, Wallet recieverWallet)
    {
        var Sendertransaction = new WalletTransaction
        {
            Amount = Price.Create(Transferedamount),
            Type = TransactionType.Depit,
            WalletId = senderWallet.Id,
        };

        var Recievertransaction = new WalletTransaction
        {
            Amount = Price.Create(Transferedamount),
            Type = TransactionType.Credit,
            WalletId = recieverWallet.Id,
        };

        await unitOfWork.GetRepository<WalletTransaction>().AddAsync(Sendertransaction);
        await unitOfWork.GetRepository<WalletTransaction>().AddAsync(Recievertransaction);

        await unitOfWork.SaveAsync();
    }
}
