namespace FIXIT.Application.IServices;

public enum PaymentWay { Paymob = 1 , Stripe = 2}
public interface IPaymentGateway
{
    PaymentWay paymentWay { get; }
    Task<string> Pay(int amountCents, string passengerid);
    Task<bool> RecieveCallback(object payload, string hmacHeader);
    Task<decimal> ExtractAmountAsync(object payload);
    Task<string> ExtractCustomerIdAsync(object payload);
}
