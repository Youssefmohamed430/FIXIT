
using FIXIT.Domain.Entities;

namespace FIXIT.Application.Servicces;

public class EscrowPaymentServiceV2 : IEscrowPaymentServiceV2
{
    private readonly Dictionary<WorkStatus, IOrderStatusHandler> _handlers;
    private readonly IUnitOfWork unitOfWork;
    private readonly IServiceManager serviceManager;
    private readonly ILogger<EscrowPaymentService> logger;
    public EscrowPaymentServiceV2(IUnitOfWork _unitOfWork, IServiceManager _serviceManager, ILogger<EscrowPaymentService> _logger,IEnumerable<IOrderStatusHandler> handlers)
    {
        logger = _logger;
        unitOfWork = _unitOfWork;
        serviceManager = _serviceManager;
        _handlers = handlers.ToDictionary(h => h.Status);
    }
    public async Task<Result<OrderDTO>> ChangeWorkOrderStatus(
        int orderId, WorkStatus newStatus)
    {
        Result<OrderDTO> result;
        var order = await unitOfWork.GetRepository<Order>()
            .FindAsync(o => o.Id == orderId,
                new[] { "Offer.ServiceProvider.User.Wallet",
                        "JobPost.Customer.User.Wallet" });

        if (order is null)
            return Result<OrderDTO>.Failure(
                new Error("Order.NotFound", "Order not found."));

        if (_handlers.TryGetValue(newStatus, out var handler))
            result = await handler.HandleAsync(order);
        else
        {
            order.WorkStatus = newStatus;
            await serviceManager.notifService.NotifyCustomerByJobPostId(order.JobPostId, $"The work status of your order has been changed to {newStatus}.");
            await serviceManager.notifService.NotifyProviderByOfferId(order.OfferId, $"The work status of the order you are working on has been changed to {newStatus}.");
            result = Result<OrderDTO>.Success(order.Adapt<OrderDTO>());
        }

        await unitOfWork.GetRepository<Order>().UpdateAsync(order);
        await unitOfWork.SaveAsync();

        return result;
    }
}
