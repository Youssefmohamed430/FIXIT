
namespace FIXIT.Application.IServices;

public interface IServiceManager
{
    public IAuthService AuthService { get; }
    public IEmailService EmailService { get; }
    public IWallettService _walletService { get; }
    public IAccountService _accountService { get; }
    public IJobPostService jobPostService { get; }
    public IOfferService offerService { get; }
    public IOrderService orderService { get; }
    public IEscrowPaymentServiceV2 escrowPaymentService { get; }
    public INotifService notifService { get; }
    public IChatService ChatService { get; }
    public IPaymentGateway paymentGateway { get; }
    public IProviderRatingService _providerRatingService { get;  }
}
