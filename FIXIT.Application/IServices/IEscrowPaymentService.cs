namespace FIXIT.Application.IServices;

public interface IEscrowPaymentService
{
    Task<Result<OrderDTO>> AcceptOrder(int orderId,decimal Price);
}
