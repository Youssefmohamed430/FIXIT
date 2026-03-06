using FIXIT.Domain.Entities;
using System.Collections.Generic;

namespace FIXIT.Application.Servicces;

public class OrderService(IUnitOfWork unitOfWork) : IOrderService
{
    public async Task<Result<List<OrderDTO>>> GetOrdersByProviderId(string Id)
    {
        var Orders = await unitOfWork.GetRepository<Order>()
            .FindAllAsync<OrderDTO>(o => o.Offer.ProviderId == Id);

        if (Orders == null)
            return Result<List<OrderDTO>>.Failure(new Error("Orders.NotFound.ProviderId", "Orders for this provider not found"));

        return Result<List<OrderDTO>>.Success(Orders.ToList());
    }
    public async Task<Result<List<OrderDTO>>> GetOrdersByCustomerId(string Id)
    {
        var Orders = await unitOfWork.GetRepository<Order>()
                    .FindAllAsync<OrderDTO>(o => o.JobPost.CustomerId == Id);

        if (Orders == null)
            return Result<List<OrderDTO>>.Failure(new Error("Orders.NotFound.CustomerId", "Orders for this customer not found"));

        return Result<List<OrderDTO>>.Success(Orders.ToList());
    }
    public async Task<Result<OrderDTO>> CreateOrder(OrderDTO order)
    {
        try
        {
            var newOrder = new Order
            {
                JobPostId = order.JobPostId,
                OfferId = order.OfferId,
                TotalAmount = order.TotalAmount
            };

            await unitOfWork.GetRepository<Order>().AddAsync(newOrder);
            await unitOfWork.SaveAsync();

            return Result<OrderDTO>.Success(order);
        }
        catch (Exception ex)
        {
            return Result<OrderDTO>.Failure(new Error("Orders.CreateFailed", $"Failed to create order: {ex.Message}"));
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
