
namespace FIXIT.Application.Servicces;

public class EscrowPaymentService(IUnitOfWork unitOfWork,IServiceManager serviceManager) : IEscrowPaymentService
{
    public async Task<Result<OrderDTO>> AcceptOrder(int orderId,decimal Price)
    {
        var order = await unitOfWork.GetRepository<Order>()
            .FindAsync(o => o.Id == orderId,new string[] { "JobPost.Customer.User.Wallet" });

        if (order is null)
            return Result<OrderDTO>.Failure(new Error("Order.NotFound","Order not found"));

        if(Price != order.TotalAmount.Amount)
            return Result<OrderDTO>.Failure(new Error("Order.InvalidPrice","Provided price does not match the order total amount"));

        var customerWalletId = order.JobPost.Customer.User.Wallet.Id;
        var TransferResult = await serviceManager._walletService.TransferMoney(customerWalletId, customerWalletId);

        if (!TransferResult.IsSuccess)
            return Result<OrderDTO>.Failure(new Error("Payment.Failed", "Failed to transfer money to escrow"));
        else
        {
            order.WorkStatus = WorkStatus.Accepted;
            order.PaymentStatus = PaymentStatus.Held;
        }
        return Result<OrderDTO>.Success(new OrderDTO
        {
            JobPostId = order.JobPostId,
            TotalAmount = order.TotalAmount,
            WorkStatus = order.WorkStatus,
            PaymentStatus = order.PaymentStatus
        });
    }
}
