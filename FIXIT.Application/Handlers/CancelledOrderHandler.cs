
using Microsoft.Extensions.Localization;

namespace FIXIT.Application.Handlers;

public class CancelledOrderHandler(
    IUnitOfWork unitOfWork,IServiceManager serviceManager,IStringLocalizer<CancelledOrderHandler> _localizer) : IOrderStatusHandler
{
    public WorkStatus Status => WorkStatus.Cancelled;
    private const int PlatformWalletId = 1;


    public async Task<Result<OrderDTO>> HandleAsync(Order order)
    {
        var customerWalletId = order.JobPost.Customer.User.Wallet.Id;

        var transferResult = await serviceManager._walletService
            .TransferMoney(
                order.Id,
                PlatformWalletId,
                customerWalletId,
                order.TotalAmount.Amount);

        if (!transferResult.IsSuccess)
        {
            order.PaymentStatus = PaymentStatus.Failed;
            return Result<OrderDTO>.Failure(
                new Error("Payment.Failed", _localizer["Wallet.PaymentFailed"] ));
        }

        order.WorkStatus = WorkStatus.Cancelled;
        order.PaymentStatus = PaymentStatus.Refunded;

        await serviceManager.notifService.NotifyCustomerByJobPostId(
            order.JobPostId, _localizer["Escrow.Cancelled"]);

        await serviceManager.notifService.NotifyProviderByOfferId(
            order.OfferId, _localizer["Escrow.CancelledProvider"]);

        return Result<OrderDTO>.Success(order.Adapt<OrderDTO>());
    }
}
