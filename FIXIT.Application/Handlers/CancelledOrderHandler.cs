using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FIXIT.Application.Handlers
{
    public class CancelledOrderHandler(
        IUnitOfWork unitOfWork,IServiceManager serviceManager) : IOrderStatusHandler
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
                    new Error("Payment.Failed", "Failed to refund money."));
            }

            order.WorkStatus = WorkStatus.Cancelled;
            order.PaymentStatus = PaymentStatus.Refunded;

            await serviceManager.notifService.NotifyCustomerByJobPostId(
                order.JobPostId, "Your order has been cancelled and payment refunded.");

            await serviceManager.notifService.NotifyProviderByOfferId(
                order.OfferId, "An order has been cancelled.");

            return Result<OrderDTO>.Success(order.Adapt<OrderDTO>());
        }
    }
}
