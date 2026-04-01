namespace FIXIT.Application.Handlers;

public interface IOrderStatusHandler
{
    const int PlatformWalletId =1 ;
    WorkStatus Status { get; }
    Task<Result<OrderDTO>> HandleAsync(Order order);
}
