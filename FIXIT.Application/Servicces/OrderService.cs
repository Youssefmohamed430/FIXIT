
namespace FIXIT.Application.Servicces;

public class OrderService(IUnitOfWork unitOfWork,IServiceManager serviceManager,ILogger<OrderService> logger) : IOrderService
{
    #region Get Orders
    public async Task<Result<List<OrderDTO>>> GetOrdersByProviderId(string Id)
    {
        var Orders = await unitOfWork.GetRepository<Order>()
            .FindAllAsync<OrderDTO>(o => o.Offer.ProviderId == Id,new string[] {"Offer"});

        if (Orders == null)
            return Result<List<OrderDTO>>.Failure(new Error("Orders.NotFound.ProviderId", "Orders for this provider not found"));

        return Result<List<OrderDTO>>.Success(Orders.ToList());
    }
    public async Task<Result<List<OrderDTO>>> GetOrdersByCustomerId(string Id)
    {
        var Orders = await unitOfWork.GetRepository<Order>()
                    .FindAllAsync<OrderDTO>(o => o.JobPost.CustomerId == Id,new string[] { "JobPost" });

        if (Orders == null)
            return Result<List<OrderDTO>>.Failure(new Error("Orders.NotFound.CustomerId", "Orders for this customer not found"));

        return Result<List<OrderDTO>>.Success(Orders.ToList());
    }
    #endregion

    #region Create - Delete

    public async Task<Result<CreateOrderDTO>> CreateOrder(CreateOrderDTO order)
    {
        try
        {
            var newOrder = order.Adapt<Order>();

            newOrder.TotalAmount = unitOfWork.GetRepository<Offer>()
                .FindAsync(o => o.Id == order.OfferId).Result.Price;
            

            await serviceManager.notifService.NotifyCustomerByJobPostId(order.JobPostId, $"Your order has been created.");
            await serviceManager.notifService.NotifyProviderByOfferId(order.OfferId, $"A new order has been created for your offer with price {newOrder.TotalAmount}.");
            
            logger.LogInformation("Creating order for JobPostId: {JobPostId} and OfferId: {OfferId}", order.JobPostId, order.OfferId);

            await unitOfWork.GetRepository<Order>().AddAsync(newOrder);
            await unitOfWork.SaveAsync();

            return Result<CreateOrderDTO>.Success(order);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to create order for JobPostId: {JobPostId} and OfferId: {OfferId}", order.JobPostId, order.OfferId);
            return Result<CreateOrderDTO>.Failure(new Error("Orders.CreateFailed", $"Failed to create order: {ex.Message}"));
        }
    }

    public async Task<Result<object>> DeleteOrder(int id)
    {
        var order = await unitOfWork.GetRepository<Order>()
            .FindAsync(o => o.Id == id,new string[] {"JobPost.Customer.User"});

        if (order == null)
        {
            logger.LogWarning("Attempted to delete order with Id: {OrderId}, but it was not found", id);
            return Result<object>.Failure(new Error("Order.NotFound","Order not found"));
        }

        order.IsDeleted = true;

        await unitOfWork.GetRepository<Order>().UpdateAsync(order);
        await unitOfWork.SaveAsync();

        await serviceManager.notifService.NotifyCustomerByJobPostId(order.JobPostId, "The request was successfully deleted");
        await serviceManager.notifService.NotifyProviderByOfferId(order.OfferId, $"Your request to view your post on {order.JobPost.Customer.User.Name}'s page has been deleted.");

        logger.LogInformation("Deleted order with Id: {OrderId}", id);
        return Result<object>.Success(order);
    }
    #endregion
}
