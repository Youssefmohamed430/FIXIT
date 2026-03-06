namespace FIXIT.Application.IServices;

public interface IOrderService
{
    Task<Result<List<OrderDTO>>> GetOrdersByProviderId(string Id);
    Task<Result<List<OrderDTO>>> GetOrdersByCustomerId(string Id);
    Task<Result<OrderDTO>> CreateOrder(OrderDTO order);
    Task<Result<Object>> DeleteOrder(int id);
}