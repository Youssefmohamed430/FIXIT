
namespace FIXIT.Application.Servicces;

public class WalletService(IUnitOfWork unitOfWork,ILogger<WalletService> logger) : IWallettService
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
            logger.LogError(ex, "Failed to create wallet for user {UserId}", walletDTO.UserId);
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

    public async Task<Result<WalletDTO>> TransferMoney(int orderid,int WalletSenderId, int WalletRecieverId,decimal Transferedamount)
    {
        try
        {
            unitOfWork.BeginTransaction();

            logger.LogInformation("Initiating transfer of {Amount} from wallet {SenderWalletId} to wallet {RecieverWalletId} for order {OrderId}", Transferedamount, WalletSenderId, WalletRecieverId, orderid);
            var senderWallet = await unitOfWork.GetRepository<Wallet>().FindAsync(w => w.Id == WalletSenderId);
            var recieverWallet = await unitOfWork.GetRepository<Wallet>().FindAsync(w => w.Id == WalletRecieverId);
            
            await HandleBalanceWallets(Transferedamount,orderid, senderWallet, recieverWallet);

            await unitOfWork.GetRepository<Wallet>().UpdateAsync(senderWallet);
            await unitOfWork.GetRepository<Wallet>().UpdateAsync(recieverWallet);

            await unitOfWork.SaveAsync();

            unitOfWork.Commit();
            return Result<WalletDTO>.Success(senderWallet.Adapt<WalletDTO>());
        }
        catch (Exception ex)
        {
            unitOfWork.Rollback();
            logger.LogError(ex, "Failed to transfer money from wallet {SenderWalletId} to wallet {RecieverWalletId} for order {OrderId}", WalletSenderId, WalletRecieverId, orderid);
            return Result<WalletDTO>.Failure(new Error("Wallet.TransferFailed", ex.Message));
        }
    }

    private async Task HandleBalanceWallets(decimal Transferedamount,int orderid, Wallet senderWallet, Wallet recieverWallet)
    {
        if(senderWallet.Balance.Amount < Transferedamount)
        {
            logger.LogWarning("Transfer failed due to insufficient balance. Sender Wallet ID: {SenderWalletId}, Available Balance: {AvailableBalance}, Attempted Transfer Amount: {TransferAmount}", senderWallet.Id, senderWallet.Balance.Amount, Transferedamount);
            throw new Exception("You does not have enough balance to transfer the specified amount.");
        }

        var Senderbalance = senderWallet.Balance.Amount;
        Senderbalance -= Transferedamount;
        senderWallet.Balance = Price.Create(Senderbalance);

        var recieverBalance = recieverWallet.Balance.Amount;
        recieverBalance += Transferedamount;
        recieverWallet.Balance = Price.Create(recieverBalance);

        await CreateTransactions(Transferedamount,orderid, senderWallet, recieverWallet);
    }

    private async Task CreateTransactions(decimal Transferedamount,int orderid, Wallet senderWallet, Wallet recieverWallet)
    {
        var Sendertransaction = new WalletTransaction
        {
            Amount = Price.Create(Transferedamount),
            Type = TransactionType.Depit,
            WalletId = senderWallet.Id,
            OrderId = orderid
        };

        var Recievertransaction = new WalletTransaction
        {
            Amount = Price.Create(Transferedamount),
            Type = TransactionType.Credit,
            WalletId = recieverWallet.Id,
            OrderId = orderid
        };

        await unitOfWork.GetRepository<WalletTransaction>().AddAsync(Sendertransaction);
        await unitOfWork.GetRepository<WalletTransaction>().AddAsync(Recievertransaction);

        await unitOfWork.SaveAsync();
    }
}
