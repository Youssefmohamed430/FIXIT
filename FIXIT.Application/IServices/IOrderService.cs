namespace FIXIT.Application.IServices;

public interface IOrderService
{
    Task<Result<List<OrderDTO>>> GetOrdersByProviderId(string Id);
    Task<Result<List<OrderDTO>>> GetOrdersByCustomerId(string Id);
    Task<Result<CreateOrderDTO>> CreateOrder(CreateOrderDTO order);
    Task<Result<Object>> DeleteOrder(int id);
}