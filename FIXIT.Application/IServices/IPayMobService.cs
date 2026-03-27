namespace FIXIT.Application.IServices;

public interface IPayMobService
{
    Task<string> PayWithCard(int amountCents,string passengerid);
    Task<bool> PaymobCallback(PaymobCallback payload, string hmacHeader);
}
