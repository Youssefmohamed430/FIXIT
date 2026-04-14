using FIXIT.Application.Servicces;
using FIXIT.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FIXIT.Application.Handlers;

public class AcceptedOrderHandler(
    IUnitOfWork unitOfWork,IServiceManager serviceManager,ILogger<AcceptedOrderHandler> logger) : IOrderStatusHandler
{
    public WorkStatus Status => WorkStatus.Accepted;
    private const int PlatformWalletId = 1;

    public async Task<Result<OrderDTO>> HandleAsync(Order order)
    {
        Result<OrderDTO> result;
        var customerWalletId = order!.JobPost!.Customer!.User!.Wallet!.Id;
        var TransferResult = await serviceManager._walletService
            .TransferMoney(order.Id, customerWalletId, PlatformWalletId, order.TotalAmount.Amount);

        if (!TransferResult.IsSuccess)
        {
            logger.LogError("Failed to transfer money to escrow for order id {OrderId}. Error: {ErrorMessage}", order.Id, TransferResult.Error.Descriprion);
            order.PaymentStatus = PaymentStatus.Failed;
            result = Result<OrderDTO>.Failure(new Error("Payment.Failed", $"Failed to transfer money to escrow because {TransferResult.Error.Descriprion}"));
            await serviceManager.notifService.NotifyCustomerByJobPostId(order.JobPostId, $"Your order has been accepted but the payment transfer to escrow failed for order id {order.Id}.");
        }
        else
        {
            logger.LogInformation("Order id {OrderId} accepted successfully. Money transferred to escrow.", order.Id);
            order.WorkStatus = WorkStatus.Accepted;
            order.PaymentStatus = PaymentStatus.Held;
            result = Result<OrderDTO>.Success(order.Adapt<OrderDTO>());
            await serviceManager.notifService.NotifyCustomerByJobPostId(order.JobPostId, $"Your order has been accepted and payment is held in escrow for order id {order.Id}.");
            await serviceManager.notifService.NotifyProviderByOfferId(order.OfferId, $"You have accepted an order with id {order.Id}. Please start working on it.");
        }

        return result;
    }
}
