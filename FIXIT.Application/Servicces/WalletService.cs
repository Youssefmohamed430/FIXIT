using FIXIT.Domain.Abstractions;
using FIXIT.Domain.Entities;
using Microsoft.AspNetCore.Mvc;

namespace FIXIT.Application.Servicces;

public class WalletService(IUnitOfWork unitOfWork,ILogger<WalletService> logger,IServiceManager servicemanager) : IWallettService
{
    public Task<string> ChargeWallet(double amount, string customerid)
    {
        return servicemanager.payMobService.PayWithCard((int)amount, customerid);
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
    public async Task<Result<object>> PaymobCallback(PaymobCallback payload, string hmacHeader)
    {
        try
        {
            logger.LogInformation("Paymob callback received with payload: {payload}", System.Text.Json.JsonSerializer.Serialize(payload));
            unitOfWork.BeginTransaction();

            if (await servicemanager.payMobService.PaymobCallback(payload, hmacHeader))
            {
                var customerid = payload.obj.payment_key_claims.billing_data.apartment;

                var notif = new NotifDTO
                {
                    UserId = customerid,
                    Message = $"Your card has been successfully debited with {Convert.ToInt32(payload.obj.amount_cents) / 100} pounds."
                };

                await servicemanager.notifService.CreateNotif(notif);

                await UpdateBalance(Convert.ToInt32(payload.obj.amount_cents) / 100, customerid);

                unitOfWork.Commit();

                return Result<object>.Success( null!);
            }
            else
            {
                unitOfWork.Commit();

                return Result<object>.Failure(new Error("Payment.Fail", "Sorry, The payment process was failed."));
            }
        }
        catch (Exception ex)
        {
            unitOfWork.Rollback();
            return Result<object>.Failure(new Error("Payment.Error", $"Sorry, An error occurred during the payment process. {ex.Message}"));
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
            Message = $"Wallet charged Successfully with {amount} pounds."
        };

        await servicemanager.notifService.CreateNotif(notif);

        return Result<WalletDTO>.Success(wallet.Adapt<WalletDTO>());
    }
}
