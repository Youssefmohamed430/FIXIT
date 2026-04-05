namespace FIXIT.Application.Handlers;

public interface IOrderStatusHandler
{
    WorkStatus Status { get; }
    Task<Result<OrderDTO>> HandleAsync(Order order);
}
