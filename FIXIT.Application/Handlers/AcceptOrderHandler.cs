
using Microsoft.Extensions.Localization;

namespace FIXIT.Application.Handlers;

public class AcceptedOrderHandler(
    IUnitOfWork unitOfWork,IServiceManager serviceManager,ILogger<AcceptedOrderHandler> logger,IStringLocalizer<AcceptedOrderHandler> _localizer) : IOrderStatusHandler
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
            result = Result<OrderDTO>.Failure(new Error("Payment.Failed", _localizer["Escrow.PaymentFailedV2", TransferResult.Error.Descriprion]));
            await serviceManager.notifService.NotifyCustomerByJobPostId(order.JobPostId, _localizer["Escrow.Accepted", order.Id]);
        }
        else
        {
            logger.LogInformation("Order id {OrderId} accepted successfully. Money transferred to escrow.", order.Id);
            order.WorkStatus = WorkStatus.Accepted;
            order.PaymentStatus = PaymentStatus.Held;
            result = Result<OrderDTO>.Success(order.Adapt<OrderDTO>());
            await serviceManager.notifService.NotifyCustomerByJobPostId(order.JobPostId, _localizer["Escrow.Accepted", order.Id]);
            await serviceManager.notifService.NotifyProviderByOfferId(order.OfferId, _localizer["Escrow.AcceptedProvider",order.Id]);
        }

        return result;
    }
}
