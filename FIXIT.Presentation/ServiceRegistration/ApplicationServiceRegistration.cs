using FIXIT.Application.Services;

namespace FIXIT.Presentation.ServiceRegistration;

public static class ApplicationServiceRegistration
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        services.AddTransient<IUnitOfWork, UnitOfWork>();

        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IWallettService, WalletService>();
        services.AddScoped<IJobPostService, JobPostService>();
        services.AddScoped<IOfferService, OfferService>();
        services.AddScoped<IEmailService, EmailService>();
        services.AddScoped<IAccountService, AccountService>();
        services.AddScoped<IOrderService, OrderService>();
        //services.AddScoped<IEscrowPaymentService, EscrowPaymentService>();
        services.AddScoped<IEscrowPaymentServiceV2, EscrowPaymentServiceV2>();
        services.AddScoped<INotifService, NotifService>();
        services.AddScoped<IPaymentGateway, PayMobService>();
        services.AddScoped<IPaymentGateway, StripeService>();
        services.AddScoped<IChatService, ChatService>();
        services.AddScoped<IServiceManager, ServiceManager>();

        services.AddScoped<IUserRoleHandler, CustomerRoleHandler>();
        services.AddScoped<IUserRoleHandler, ProviderRoleHandler>();
        services.AddScoped<IOrderStatusHandler, AcceptedOrderHandler>();
        services.AddScoped<IOrderStatusHandler, CancelledOrderHandler>();
        services.AddScoped<IOrderStatusHandler, CompletedOrderHandler>();

        services.AddScoped<RegisterUserService>();
        services.AddScoped<JWTService>();

        return services;
    }
}
