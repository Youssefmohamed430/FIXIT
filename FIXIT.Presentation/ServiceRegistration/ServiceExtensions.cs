using FIXIT.Presentation.ServiceRegistration;

namespace Service_Layer.ServiceRegistration;

public static class ServiceExtensions
{
    public static IServiceCollection AddCoreApplicationServices(this IServiceCollection services, IConfiguration config)
    {
        var appsettingsconfig = new ConfigurationBuilder()
            .AddJsonFile("appsettings.json")
            .Build();
        
        var connectionString = Environment.GetEnvironmentVariable("constr");

        services.AddControllerServices();
        services.AddDatabaseServices(connectionString);
        services.AddApplicationServices();
        services.AddJwtAuthentication(config);
        services.AddCorsServices();
        services.AddIdentityServices();
        services.AddLocalizationServices();

        return services;
    }

}
