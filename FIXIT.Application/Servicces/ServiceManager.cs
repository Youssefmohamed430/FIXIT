
namespace FIXIT.Application.Servicces;

public class ServiceManager : IServiceManager
{
    private readonly IServiceProvider _serviceProvider;

    public IUnitOfWork UnitOfWork { get; }
    public UserManager<ApplicationUser> Usermanager { get; set; }

    #region Lazy Services
    private readonly Lazy<IAuthService> _authService;
    private readonly Lazy<IEmailService> _emailService;
    private readonly Lazy<IWallettService> _walletService;
    private readonly Lazy<IAccountService> _accountService;
    private readonly Lazy<IJobPostService> _jobPostService;
    private readonly Lazy<IOfferService> _offerService;
    private readonly Lazy<IOrderService> _orderService;
    //private readonly Lazy<IEscrowPaymentService> _escrowPaymentService;
    private readonly Lazy<IEscrowPaymentServiceV2> _escrowPaymentService;
    private readonly Lazy<INotifService> _notifService;
    private readonly Lazy<IPayMobService> _paymobservice;
    private readonly Lazy<IPaymentGateway> _paymentGateway;


    #endregion

    #region Constructor
    public ServiceManager(
        IServiceProvider serviceProvider,
        IUnitOfWork unitOfWork,
        UserManager<ApplicationUser> userManager)
    {
        _serviceProvider = serviceProvider;
        Usermanager = userManager;
        UnitOfWork = unitOfWork;

        _authService = new Lazy<IAuthService>(
            () => _serviceProvider.GetRequiredService<IAuthService>()
        );

        _emailService = new Lazy<IEmailService>(
            () => _serviceProvider.GetRequiredService<IEmailService>()
        );

        _walletService = new Lazy<IWallettService>(
            () => _serviceProvider.GetRequiredService<IWallettService>()
        );

        _accountService = new Lazy<IAccountService>(
            () => _serviceProvider.GetRequiredService<IAccountService>()
        );

        _jobPostService = new Lazy<IJobPostService>(
            () => _serviceProvider.GetRequiredService<IJobPostService>()
        );

        _offerService = new Lazy<IOfferService>(
            () => _serviceProvider.GetRequiredService<IOfferService>()
        );

        _orderService = new Lazy<IOrderService>(
            () => _serviceProvider.GetRequiredService<IOrderService>()
        );
        _escrowPaymentService = new Lazy<IEscrowPaymentServiceV2>(
            () => _serviceProvider.GetRequiredService<IEscrowPaymentServiceV2>()
        );
        _notifService = new Lazy<INotifService>(
            () => _serviceProvider.GetRequiredService<INotifService>()
        );
        _paymobservice = new Lazy<IPayMobService>(
            () => _serviceProvider.GetRequiredService<IPayMobService>()
        );
        _paymentGateway = new Lazy<IPaymentGateway>(
            () => _serviceProvider.GetRequiredService<IPaymentGateway>()
        );
    }
    #endregion

    #region Service Properties
    public IAuthService AuthService => _authService.Value;
    public IEmailService EmailService => _emailService.Value;
    IAccountService IServiceManager._accountService => _accountService.Value;
    IWallettService IServiceManager._walletService => _walletService.Value;

    public IJobPostService jobPostService => _jobPostService.Value;
    public IOfferService offerService => _offerService.Value;
    public IOrderService orderService => _orderService.Value;
    public IEscrowPaymentServiceV2 escrowPaymentService => _escrowPaymentService.Value;
    public INotifService notifService => _notifService.Value;
    public IPayMobService payMobService => _paymobservice.Value;
    public IPaymentGateway paymentGateway => _paymentGateway.Value;
    #endregion
}