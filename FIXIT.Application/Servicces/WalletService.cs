using Microsoft.Extensions.Localization;

namespace FIXIT.Application.Servicces;

public class WalletService
    (IUnitOfWork unitOfWork,ILogger<WalletService> logger,
    IServiceManager servicemanager,IEnumerable<IPaymentGateway> PayHandler
    ,IStringLocalizer<WalletService> _localizer) : IWallettService
{
    public async Task<string> ChargeWallet(double amount, string customerid,PaymentWay paymentWay)
    {
        var handler = PayHandler.FirstOrDefault(p => p.paymentWay == paymentWay);

        return await handler.Pay((int)amount, customerid);
    }
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
        var wallet = await unitOfWork.GetRepository<Wallet>()
            .FindAsync(w => w.UserId == userId,new string[] {"User"});

        if (wallet == null)
            return Result<WalletDTO>.Failure(new Error("Wallet.NotFound", _localizer["Wallet.NotFound"]));
        
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
            
            await UpdateBalance(Transferedamount,orderid, senderWallet, recieverWallet);

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
            return Result<WalletDTO>.Failure(new Error("Wallet.TransferFailed", _localizer["Wallet.TransferFailed",ex.Message]));
        }
    }

    private async Task UpdateBalance(decimal Transferedamount,int orderid, Wallet senderWallet, Wallet recieverWallet)
    {
        if(senderWallet.Balance.Amount < Transferedamount)
        {
            logger.LogWarning("Transfer failed due to insufficient balance. Sender Wallet ID: {SenderWalletId}, Available Balance: {AvailableBalance}, Attempted Transfer Amount: {TransferAmount}", senderWallet.Id, senderWallet.Balance.Amount, Transferedamount);
            throw new Exception(_localizer["Wallet.InsufficientBalance"]);
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
    public async Task<Result<object>> RecieveCallback(
    object payload,
    Dictionary<string, string> headers,
    PaymentWay paymentWay)
    {
        try
        {
            logger.LogInformation("Callback received for {PaymentWay}", paymentWay);
            unitOfWork.BeginTransaction();

            var handler = PayHandler.FirstOrDefault(p => p.paymentWay == paymentWay);

            if (await handler.RecieveCallback(payload, headers)) 
            {
                var customerid = await handler.ExtractCustomerIdAsync(payload);
                var amount = await handler.ExtractAmountAsync(payload);

                var notif = new NotifDTO
                {
                    UserId = customerid,
                    Message = _localizer["Wallet.CardDebited",amount]
                };

                await servicemanager.notifService.CreateNotif(notif);
                await UpdateBalance(amount, customerid);

                unitOfWork.Commit();
                return Result<object>.Success(null!);
            }
            else
            {
                unitOfWork.Commit();
                return Result<object>.Failure(
                    new Error("Payment.Fail", _localizer["Wallet.PaymentFailed"]));
            }
        }
        catch (Exception ex)
        {
            unitOfWork.Rollback();
            return Result<object>.Failure(
                new Error("Payment.Error", _localizer["Payment.Error",ex.Message]));
        }
    }
    public async Task<Result<WalletDTO>> UpdateBalance(decimal amount, string Customerid)
    {
        var wallet = await unitOfWork.GetRepository<Wallet>()
            .FindAsync(w => w.UserId == Customerid);


         var walletbalance = wallet.Balance.Amount;
         walletbalance += amount;
         wallet.Balance = Price.Create(walletbalance);
         await unitOfWork.GetRepository<Wallet>().UpdateAsync(wallet);
         await unitOfWork.SaveAsync();

        var notif = new NotifDTO
        {
            UserId = Customerid,
            Message = _localizer["Wallet.ChargedSuccess"]
        };

        await servicemanager.notifService.CreateNotif(notif);

        return Result<WalletDTO>.Success(wallet.Adapt<WalletDTO>());
    }

    public async Task<Result<WalletDTO>> Withdraw(WithdrawDTO withdrawDTO)
    {
        try
        {
            var wallet = await unitOfWork.GetRepository<Wallet>()
            .FindAsync(w => w.Id == withdrawDTO.WalletId,new string[] {"User"});

            if (wallet.Balance.Amount < withdrawDTO.Amount)
            {
                return Result<WalletDTO>.Failure(new Error("Wallet.InsufficientBalance", _localizer["Wallet.InsufficientBalance"]));
            }

            var walletbalance = wallet.Balance.Amount;
            walletbalance -= withdrawDTO.Amount;
            wallet.Balance = Price.Create(walletbalance);

            await unitOfWork.GetRepository<Wallet>().UpdateAsync(wallet);
            await unitOfWork.SaveAsync();

            await SendNotif(withdrawDTO.MobileNumber, withdrawDTO.Amount, wallet);

            return Result<WalletDTO>.Success(wallet.Adapt<WalletDTO>());
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to withdraw money from wallet {WalletId} to mobile number {MobileNumber}", withdrawDTO.WalletId, withdrawDTO.MobileNumber);
            return Result<WalletDTO>.Failure(new Error("Wallet.WithdrawalFailed", _localizer["Wallet.WithdrawalFailed",ex.Message]));
        }
    }

    private async Task SendNotif(string mobileNumber, decimal amount, Wallet wallet)
    {
        var notif = new NotifDTO
        {
            UserId = wallet.UserId,
            Message = _localizer["Wallet.WithdrawSuccess",amount,mobileNumber]
        };

        await servicemanager.notifService.CreateNotif(notif);
    }
}
