namespace FIXIT.Application.IServices;

public interface IEscrowPaymentServiceV2
{
    Task<Result<OrderDTO>> ChangeWorkOrderStatus(int id, WorkStatus workStatus);
}
