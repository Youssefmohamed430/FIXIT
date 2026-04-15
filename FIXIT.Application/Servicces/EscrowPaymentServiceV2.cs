
using FIXIT.Domain.Entities;
using Microsoft.Extensions.Localization;

namespace FIXIT.Application.Servicces;

public class EscrowPaymentServiceV2 : IEscrowPaymentServiceV2
{
    private readonly Dictionary<WorkStatus, IOrderStatusHandler> _handlers;
    private readonly IUnitOfWork unitOfWork;
    private readonly IServiceManager serviceManager;
    private readonly ILogger<EscrowPaymentService> logger;
    private IStringLocalizer<EscrowPaymentServiceV2> _localizer;
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
                new Error("Order.NotFound", _localizer["Order.NotFound"]));

        if (_handlers.TryGetValue(newStatus, out var handler))
            result = await handler.HandleAsync(order);
        else
        {
            order.WorkStatus = newStatus;
            await serviceManager.notifService.NotifyCustomerByJobPostId(order.JobPostId, _localizer["Escrow.StatusChanged",newStatus] );
            await serviceManager.notifService.NotifyProviderByOfferId(order.OfferId, _localizer["Escrow.StatusChanged", newStatus]);
            result = Result<OrderDTO>.Success(order.Adapt<OrderDTO>());
        }

        await unitOfWork.GetRepository<Order>().UpdateAsync(order);
        await unitOfWork.SaveAsync();

        return result;
    }
}
