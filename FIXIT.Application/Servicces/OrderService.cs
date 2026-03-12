
namespace FIXIT.Application.Servicces;

public class OrderService(IUnitOfWork unitOfWork) : IOrderService
{
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
    public async Task<Result<CreateOrderDTO>> CreateOrder(CreateOrderDTO order)
    {
        try
        {
            var newOrder = order.Adapt<Order>();

            newOrder.TotalAmount = unitOfWork.GetRepository<Offer>()
                .FindAsync(o => o.Id == order.OfferId).Result.Price;

            await unitOfWork.GetRepository<Order>().AddAsync(newOrder);
            await unitOfWork.SaveAsync();

            return Result<CreateOrderDTO>.Success(order);
        }
        catch (Exception ex)
        {
            return Result<CreateOrderDTO>.Failure(new Error("Orders.CreateFailed", $"Failed to create order: {ex.Message}"));
        }
    }

    public async Task<Result<object>> DeleteOrder(int id)
    {
        var order = await unitOfWork.GetRepository<Order>().FindAsync(o => o.Id == id);

        if (order == null)
            return Result<object>.Failure(new Error("Order.NotFound","Order not found"));

        order.IsDeleted = true;

        await unitOfWork.GetRepository<Order>().UpdateAsync(order);
        await unitOfWork.SaveAsync();

        return Result<object>.Success(order);
    }
}
