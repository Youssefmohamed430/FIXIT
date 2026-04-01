using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FIXIT.Application.Handlers
{
    public class CompletedOrderHandler(
        IUnitOfWork unitOfWork,IServiceManager serviceManager) : IOrderStatusHandler
    {
        public WorkStatus Status => WorkStatus.Completed;
        private const int PlatformWalletId = 1;


        public async Task<Result<OrderDTO>> HandleAsync(Order order)
        {
            var providerWalletId = order.Offer.ServiceProvider.User.Wallet.Id;

            // احسب الـ Commission
            var platformAmount = order.TotalAmount.Amount * 10 / 100;
            var providerAmount = order.TotalAmount.Amount - platformAmount;

            order.ProviderAmount = Price.Create(providerAmount);
            order.PlatformCommission = Price.Create(platformAmount);

            var transferResult = await serviceManager._walletService
                .TransferMoney(
                    order.Id,
                    PlatformWalletId,
                    providerWalletId,
                    providerAmount);

            if (!transferResult.IsSuccess)
            {
                order.PaymentStatus = PaymentStatus.Failed;
                return Result<OrderDTO>.Failure(
                    new Error("Payment.Failed", "Failed to transfer money to provider."));
            }

            order.WorkStatus = WorkStatus.Completed;
            order.PaymentStatus = PaymentStatus.Paid;

            await serviceManager.notifService.NotifyCustomerByJobPostId(
                order.JobPostId, "Your order has been completed.");

            await serviceManager.notifService.NotifyProviderByOfferId(
                order.OfferId, "Payment has been transferred to your wallet.");

            return Result<OrderDTO>.Success(order.Adapt<OrderDTO>());
        }
    }
}
