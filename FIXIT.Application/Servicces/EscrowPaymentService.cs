namespace FIXIT.Application.Servicces;

public class EscrowPaymentService(IUnitOfWork unitOfWork,IServiceManager serviceManager) : IEscrowPaymentService
{
    private const int PlatformWalletId = 1;
    public async Task<Result<OrderDTO>> AcceptOrder(int orderId)
    {
        var order = await unitOfWork.GetRepository<Order>()
            .FindAsync(o => o.Id == orderId,new string[] { "JobPost.Customer.User.Wallet" });

        Result<OrderDTO> result;
        try
        {
            if (order is null)
                return Result<OrderDTO>.Failure(new Error("Order.NotFound", "Order not found"));

            var customerWalletId = order!.JobPost!.Customer!.User!.Wallet!.Id;
            var TransferResult = await serviceManager._walletService
                .TransferMoney(order.Id,customerWalletId, PlatformWalletId, order.TotalAmount.Amount);

            if (!TransferResult.IsSuccess)
            {
                order.PaymentStatus = PaymentStatus.Failed;
                result = Result<OrderDTO>.Failure(new Error("Payment.Failed", "Failed to transfer money to escrow"));
                await SendNotificcationToCustomer(order.JobPostId, "Your order payment failed. Please try again.");
            }
            else
            {
                order.WorkStatus = WorkStatus.Accepted;
                order.PaymentStatus = PaymentStatus.Held;
                result = Result<OrderDTO>.Success(order.Adapt<OrderDTO>());
                await SendNotificcationToCustomer(order.JobPostId, "Your order has been accepted and payment is held in escrow.");
                await SendNotificationToProvider(order.OfferId, "You have accepted an order. Please start working on it.");
            }
            await unitOfWork.GetRepository<Order>().UpdateAsync(order);
            await unitOfWork.SaveAsync();

            return result;
        }
        catch (Exception ex)
        {
            return Result<OrderDTO>.Failure(new Error("Order.AcceptedFailed", ex.Message));
        }
    }

    public async Task<Result<OrderDTO>> CancelOrder(int orderId)
    {
        Result<OrderDTO> result;
        var order = await unitOfWork.GetRepository<Order>()
            .FindAsync(o => o.Id == orderId, new string[] { "JobPost.Customer.User.Wallet" });
        try
        {
            if (order is null)
                return Result<OrderDTO>.Failure(new Error("Order.NotFound", "Order not found"));

            var customerWalletId = order!.JobPost!.Customer!.User!.Wallet!.Id;
            var TransferResult = await serviceManager._walletService
                .TransferMoney(order.Id, customerWalletId, customerWalletId, order.TotalAmount.Amount);

            if (!TransferResult.IsSuccess)
            {
                order.PaymentStatus = PaymentStatus.Failed;
                result = Result<OrderDTO>.Failure(new Error("Payment.Failed", "Failed to transfer money from escrow"));
                await SendNotificcationToCustomer(order.JobPostId, "Your order cancellation failed. Please Try again.");  
            }
            else
            {
                order.WorkStatus = WorkStatus.Cancelled;
                order.PaymentStatus = PaymentStatus.Refunded;
                result = Result<OrderDTO>.Success(order.Adapt<OrderDTO>());
                await SendNotificcationToCustomer(order.JobPostId, "Your order has been cancelled and payment is refunded.");
                await SendNotificationToProvider(order.OfferId, "An order has been cancelled. The payment is refunded to the customer.");
            }

            await unitOfWork.GetRepository<Order>().UpdateAsync(order);
            await unitOfWork.SaveAsync();

            return result ;
        }
        catch (Exception ex)
        {
            return Result<OrderDTO>.Failure(new Error("Order.CancelFailed", ex.Message));
        }
    }


    public async Task<Result<OrderDTO>> ChangeWorkOrderStatus(int id, WorkStatus workStatus)
    {
        var order = await unitOfWork.GetRepository<Order>()
            .FindAsync(o => o.Id == id,
            new string[] { "Offer.ServiceProvider.User.Wallet" });

        Result<OrderDTO> result = Result<OrderDTO>.Success(order.Adapt<OrderDTO>());

        if (order is null)
            return Result<OrderDTO>.Failure(new Error("Order.NotFound", "Order not found"));

        order!.WorkStatus = workStatus;
        await SendNotificcationToCustomer(order.JobPostId, $"The work status of your order has been changed to {workStatus}.");
        await SendNotificationToProvider(order.OfferId, $"The work status of the order you are working on has been changed to {workStatus}.");
        try
        {
            if(workStatus == WorkStatus.Completed)
            {
                result = await HandleCompletedOrder(order, result);
            }

            await unitOfWork.GetRepository<Order>().UpdateAsync(order);
            await unitOfWork.SaveAsync();

            await SendNotificcationToCustomer(order.JobPostId, $"The payment to provider is paid.");
            await SendNotificationToProvider(order.OfferId, $"The payment for the order you completed has been paid to your wallet.");

            result = Result<OrderDTO>.Success(order.Adapt<OrderDTO>());

            return result;
        }
        catch (Exception ex)
        {
            return Result<OrderDTO>.Failure(new Error("Order.UpdateFailed", ex.Message));
        }
    }

    private async Task<Result<OrderDTO>> HandleCompletedOrder(Order order, Result<OrderDTO> result)
    {
        var providerWalletId = order.Offer!.ServiceProvider!.User!.Wallet!.Id;

        HandleMoney(order);

        var TransferResult = await serviceManager._walletService
            .TransferMoney(order.Id,PlatformWalletId, providerWalletId, order.ProviderAmount.Amount);

        if (!TransferResult.IsSuccess)
        {
            order.PaymentStatus = PaymentStatus.Failed;
            result = Result<OrderDTO>.Failure(new Error("Payment.Failed", "Failed to transfer money to provider"));
        }
        else
        {
            order.PaymentStatus = PaymentStatus.Paid;
            result = Result<OrderDTO>.Success(order.Adapt<OrderDTO>());
        }

        return result;
    }
    private static void HandleMoney(Order order)
    {
        var PlatformPercentAmount = order.TotalAmount.Amount * 10 / 100;
        var ProviderAmount = order.TotalAmount.Amount - PlatformPercentAmount;
        order.ProviderAmount = Price.Create(ProviderAmount);
        order.PlatformCommission = Price.Create(PlatformPercentAmount);
    }

    private async Task SendNotificcationToCustomer(int jobpostid, string msg)
    {
        var customerId = unitOfWork.GetRepository<JobPost>()
                        .FindAsync(j => j.Id == jobpostid).Result.CustomerId;


        await serviceManager.notifService.CreateNotif(new NotifDTO
        {
            UserId = customerId,
            Message = msg
        });
    }

    private async Task SendNotificationToProvider(int offerid, string msg)
    {
        var providerId = unitOfWork.GetRepository<Offer>()
            .FindAsync(o => o.Id == offerid).Result.ProviderId;

        await serviceManager.notifService.CreateNotif(new NotifDTO
        {
            UserId = providerId,
            Message = msg
        });
    }
}
