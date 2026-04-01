namespace FIXIT.Application.IServices;

public interface IPaymentGateway
{
    Task<string> Pay(int amountCents, string passengerid);
    Task<bool> RecieveCallback(object payload, string hmacHeader);
    Task<decimal> ExtractAmountAsync(object payload);
    Task<string> ExtractCustomerIdAsync(object payload);
}
