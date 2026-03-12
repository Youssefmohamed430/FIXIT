namespace FIXIT.Application.Servicces;

public class EscrowPaymentService(IUnitOfWork unitOfWork,IServiceManager serviceManager) : IEscrowPaymentService
{
    private const int PlatformWalletId = 4;
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
            }
            else
            {
                order.WorkStatus = WorkStatus.Accepted;
                order.PaymentStatus = PaymentStatus.Held;
                result = Result<OrderDTO>.Success(order.Adapt<OrderDTO>());
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
                result = Result<OrderDTO>.Failure(new Error("Payment.Failed", "Failed to transfer money to escrow"));
            }
            else
            {
                order.WorkStatus = WorkStatus.Cancelled;
                order.PaymentStatus = PaymentStatus.Refunded;
                result = Result<OrderDTO>.Success(order.Adapt<OrderDTO>());
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
        var order = await unitOfWork.GetRepository<Order>().FindAsync(o => o.Id == id);

        Result<OrderDTO> result = Result<OrderDTO>.Success(order.Adapt<OrderDTO>());

        if (order is null)
            return Result<OrderDTO>.Failure(new Error("Order.NotFound", "Order not found"));

        order!.WorkStatus = workStatus;

        try
        {
            if(workStatus == WorkStatus.Completed)
            {
                result = await HandleCompletedOrder(order, result);
            }

            await unitOfWork.GetRepository<Order>().UpdateAsync(order);
            await unitOfWork.SaveAsync();

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
}
