
namespace FIXIT.Application.Servicces;

public class OrderService(IUnitOfWork unitOfWork,IServiceManager serviceManager) : IOrderService
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

            await SendNotificcationToCustomer(order.JobPostId, $"Your order has been created.");

            await SendNotificationToProvider(order.OfferId, $"A new order has been created for your offer with price.");

            await unitOfWork.GetRepository<Order>().AddAsync(newOrder);
            await unitOfWork.SaveAsync();

            return Result<CreateOrderDTO>.Success(order);
        }
        catch (Exception ex)
        {
            return Result<CreateOrderDTO>.Failure(new Error("Orders.CreateFailed", $"Failed to create order: {ex.Message}"));
        }
    }

    private async Task SendNotificcationToCustomer(int jobpostid,string msg)
    {
        var customerId = unitOfWork.GetRepository<JobPost>()
                        .FindAsync(j => j.Id == jobpostid).Result.CustomerId;


        await serviceManager.notifService.CreateNotif(new NotifDTO
        {
            UserId = customerId,
            Message = msg
        });
    }

    private async Task SendNotificationToProvider(int offerid,string msg)
    {
        var providerId = unitOfWork.GetRepository<Offer>()
            .FindAsync(o => o.Id == offerid).Result.ProviderId;

        await serviceManager.notifService.CreateNotif(new NotifDTO
        {
            UserId = providerId,
            Message = msg
        });
    }

    public async Task<Result<object>> DeleteOrder(int id)
    {
        var order = await unitOfWork.GetRepository<Order>()
            .FindAsync(o => o.Id == id,new string[] {"JobPost.Customer.User"});

        if (order == null)
            return Result<object>.Failure(new Error("Order.NotFound","Order not found"));

        order.IsDeleted = true;

        await unitOfWork.GetRepository<Order>().UpdateAsync(order);
        await unitOfWork.SaveAsync();

        await SendNotificcationToCustomer(order.JobPostId, "The request was successfully deleted");
        await SendNotificationToProvider(order.OfferId, $"Your request to view your post on {order.JobPost.Customer.User.Name}'s page has been deleted.");

        return Result<object>.Success(order);
    }
    #endregion
}
