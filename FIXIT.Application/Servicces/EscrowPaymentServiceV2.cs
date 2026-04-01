
namespace FIXIT.Application.Servicces;

public class EscrowPaymentServiceV2(IUnitOfWork unitOfWork,IServiceManager serviceManager,ILogger<EscrowPaymentService> logger) : IEscrowPaymentServiceV2
{
    private readonly Dictionary<WorkStatus, IOrderStatusHandler> _handlers;
    public async Task<Result<OrderDTO>> ChangeWorkOrderStatus(
        int orderId, WorkStatus newStatus)
    {
        var order = await unitOfWork.GetRepository<Order>()
            .FindAsync(o => o.Id == orderId,
                new[] { "Offer.ServiceProvider.User.Wallet",
                        "JobPost.Customer.User.Wallet" });

        if (order is null)
            return Result<OrderDTO>.Failure(
                new Error("Order.NotFound", "Order not found."));

        if (!_handlers.TryGetValue(newStatus, out var handler))
            return Result<OrderDTO>.Failure(
                new Error("Order.InvalidStatus", "Invalid status."));

        var result = await handler.HandleAsync(order);

        await unitOfWork.GetRepository<Order>().UpdateAsync(order);
        await unitOfWork.SaveAsync();

        return result;
    }
}
