namespace FIXIT.Application.Servicces;

public class EscrowPaymentService(IUnitOfWork unitOfWork,IServiceManager serviceManager,ILogger<EscrowPaymentService> logger) : IEscrowPaymentService
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
            {
                logger.LogWarning("Attempt to accept order with id {OrderId} failed because the order was not found.", orderId);
                return Result<OrderDTO>.Failure(new Error("Order.NotFound", "Order not found"));
            }

            var customerWalletId = order!.JobPost!.Customer!.User!.Wallet!.Id;
            var TransferResult = await serviceManager._walletService
                .TransferMoney(order.Id,customerWalletId, PlatformWalletId, order.TotalAmount.Amount);

            if (!TransferResult.IsSuccess)
            {
                logger.LogError("Failed to transfer money to escrow for order id {OrderId}. Error: {ErrorMessage}", orderId, TransferResult.Error.Descriprion);
                order.PaymentStatus = PaymentStatus.Failed;
                result = Result<OrderDTO>.Failure(new Error("Payment.Failed", "Failed to transfer money to escrow"));
                await serviceManager.notifService.NotifyCustomerByJobPostId(order.JobPostId, $"Your order has been accepted but the payment transfer to escrow failed for order id {orderId}.");
            }
            else
            {
                logger.LogInformation("Order id {OrderId} accepted successfully. Money transferred to escrow.", orderId);
                order.WorkStatus = WorkStatus.Accepted;
                order.PaymentStatus = PaymentStatus.Held;
                result = Result<OrderDTO>.Success(order.Adapt<OrderDTO>());
                await serviceManager.notifService.NotifyCustomerByJobPostId(order.JobPostId, $"Your order has been accepted and payment is held in escrow for order id {orderId}.");
                await serviceManager.notifService.NotifyProviderByOfferId(order.OfferId, $"You have accepted an order with id {orderId}. Please start working on it.");
            }
            await unitOfWork.GetRepository<Order>().UpdateAsync(order);
            await unitOfWork.SaveAsync();

            return result;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error occurred while accepting order with id {OrderId}.", orderId);
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
                logger.LogError("Failed to transfer money back to customer for order id {OrderId}. Error: {ErrorMessage}", orderId, TransferResult.Error.Descriprion);
                order.PaymentStatus = PaymentStatus.Failed;
                result = Result<OrderDTO>.Failure(new Error("Payment.Failed", "Failed to transfer money from escrow"));
                await serviceManager.notifService.NotifyCustomerByJobPostId(order.JobPostId, $"Your order cancellation failed for order id {orderId} because the refund transfer from escrow failed.");
            }
            else
            {
                logger.LogInformation("Order id {OrderId} cancelled successfully. Money transferred back to customer.", orderId);
                order.WorkStatus = WorkStatus.Cancelled;
                order.PaymentStatus = PaymentStatus.Refunded;
                result = Result<OrderDTO>.Success(order.Adapt<OrderDTO>());
                await serviceManager.notifService.NotifyCustomerByJobPostId(order.JobPostId, $"Your order has been cancelled and payment is refunded for order id {orderId}.");
                await serviceManager.notifService.NotifyProviderByOfferId(order.OfferId, $"An order with id {orderId} has been cancelled. The payment is refunded to the customer.");
            }

            await unitOfWork.GetRepository<Order>().UpdateAsync(order);
            await unitOfWork.SaveAsync();

            return result ;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error occurred while cancelling order with id {OrderId}.", orderId);
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
        await serviceManager.notifService.NotifyCustomerByJobPostId(order.JobPostId, $"The work status of your order has been changed to {workStatus}.");
        await serviceManager.notifService.NotifyProviderByOfferId(order.OfferId, $"The work status of the order you are working on has been changed to {workStatus}.");
        try
        {
            if(workStatus == WorkStatus.Completed)
            {
                logger.LogInformation("Order id {OrderId} marked as completed. Processing payment to provider.", id);
                result = await HandleCompletedOrder(order, result);
            }

            await unitOfWork.GetRepository<Order>().UpdateAsync(order);
            await unitOfWork.SaveAsync();

            await serviceManager.notifService.NotifyCustomerByJobPostId(order.JobPostId, $"The work status of your order has been changed to {workStatus}.");
            await serviceManager.notifService.NotifyProviderByOfferId(order.OfferId, $"The work status of the order you are working on has been changed to {workStatus}.");

            result = Result<OrderDTO>.Success(order.Adapt<OrderDTO>());

            return result;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error occurred while changing work status for order with id {OrderId}.", id);
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
            logger.LogError("Failed to transfer money to provider for order id {OrderId}. Error: {ErrorMessage}", order.Id, TransferResult.Error.Descriprion);
            order.PaymentStatus = PaymentStatus.Failed;
            result = Result<OrderDTO>.Failure(new Error("Payment.Failed", "Failed to transfer money to provider"));
        }
        else
        {
            logger.LogInformation("Money transferred to provider for order id {OrderId}.", order.Id);
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
