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
        services.AddScoped<IEscrowPaymentService, EscrowPaymentService>();
        services.AddScoped<INotifService, NotifService>();
        services.AddScoped<IPayMobService, PayMobService>();
        services.AddScoped<IServiceManager, ServiceManager>();

        services.AddScoped<IUserRoleHandler, CustomerRoleHandler>();
        services.AddScoped<IUserRoleHandler, ProviderRoleHandler>();

        services.AddScoped<RegisterUserService>();
        services.AddScoped<JWTService>();

        return services;
    }
}
