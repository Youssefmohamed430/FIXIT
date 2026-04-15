
using Microsoft.Extensions.Localization;

namespace FIXIT.Application.Servicces;

public class OrderService(IUnitOfWork unitOfWork,IServiceManager serviceManager,ILogger<OrderService> logger,IStringLocalizer<OrderService> _localizer) : IOrderService
{
    #region Get Orders
    public async Task<Result<List<OrderDTO>>> GetOrdersByProviderId(string Id)
    {
        var Orders = await unitOfWork.GetRepository<Order>()
            .FindAllAsync<OrderDTO>(o => o.Offer.ProviderId == Id && !o.IsDeleted,new string[] {"Offer"});

        if (Orders == null)
            return Result<List<OrderDTO>>.Failure(new Error("Orders.NotFound.ProviderId", _localizer["Orders.NotFound.ProviderId"]));

        return Result<List<OrderDTO>>.Success(Orders.ToList());
    }
    public async Task<Result<List<OrderDTO>>> GetOrdersByCustomerId(string Id)
    {
        var Orders = await unitOfWork.GetRepository<Order>()
                    .FindAllAsync<OrderDTO>(o => o.JobPost.CustomerId == Id && !o.IsDeleted,new string[] { "JobPost" });

        if (Orders == null)
            return Result<List<OrderDTO>>.Failure(new Error("Orders.NotFound.CustomerId", _localizer["Orders.NotFound.CustomerId"]));

        return Result<List<OrderDTO>>.Success(Orders.ToList());
    }
    #endregion

    #region Create - Delete

    public async Task<Result<CreateOrderDTO>> CreateOrder(CreateOrderDTO order)
    {
        try
        {
            var newOrder = order.Adapt<Order>();

            var offer = unitOfWork.GetRepository<Offer>()
                .FindAsync(o => o.Id == order.OfferId).Result;

            newOrder.TotalAmount = offer.Price;

            await serviceManager.notifService.NotifyCustomerByJobPostId(order.JobPostId, _localizer["Order.CustomerCreated"]);
            await serviceManager.notifService.NotifyProviderByOfferId(order.OfferId, _localizer["Order.NewOffer",newOrder.TotalAmount]);
            
            logger.LogInformation("Creating order for JobPostId: {JobPostId} and OfferId: {OfferId}", order.JobPostId, order.OfferId);
            
            offer.status = OfferStatus.Accepted;

            await unitOfWork.GetRepository<Order>().AddAsync(newOrder);
            await unitOfWork.GetRepository<Offer>().UpdateAsync(offer);
            await unitOfWork.SaveAsync();

            return Result<CreateOrderDTO>.Success(order);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to create order for JobPostId: {JobPostId} and OfferId: {OfferId}", order.JobPostId, order.OfferId);
            return Result<CreateOrderDTO>.Failure(new Error("Orders.CreateFailed", _localizer["Orders.CreateFailed",ex.Message]));
        }
    }

    public async Task<Result<object>> DeleteOrder(int id)
    {
        var order = await unitOfWork.GetRepository<Order>()
            .FindAsync(o => o.Id == id,new string[] {"JobPost.Customer.User"});

        if (order == null)
        {
            logger.LogWarning("Attempted to delete order with Id: {OrderId}, but it was not found", id);
            return Result<object>.Failure(new Error("Order.NotFound", _localizer["Order.NotFound"]));
        }

        order.IsDeleted = true;

        await unitOfWork.GetRepository<Order>().UpdateAsync(order);
        await unitOfWork.SaveAsync();

        await serviceManager.notifService.NotifyCustomerByJobPostId(order.JobPostId, _localizer["Order.Deleted"]);
        await serviceManager.notifService.NotifyProviderByOfferId(order.OfferId, _localizer["Order.DeletedNotifProvider", order.JobPost.Customer.User.Name]);

        logger.LogInformation("Deleted order with Id: {OrderId}", id);
        return Result<object>.Success(null!);
    }
    #endregion
}
