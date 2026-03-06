namespace FIXIT.Application.IServices;

public interface IEscrowPaymentService
{
    Task<Result<OrderDTO>> AcceptOrder(int orderId);
    Task<Result<OrderDTO>> CancelOrder(int orderId);
    Task<Result<OrderDTO>> ChangeWorkOrderStatus(int id, WorkStatus workStatus);
}
