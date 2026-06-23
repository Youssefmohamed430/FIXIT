namespace FIXIT.Presentation.ServiceRegistration;

public static class CorsServiceRegistration
{
    public static IServiceCollection AddCorsServices(
    this IServiceCollection services,
    IConfiguration configuration)
    {
        var allowedOrigins =
            configuration.GetSection("Cors:AllowedOrigins")
                         .Get<string[]>() ?? [];

        services.AddCors(options =>
        {
            options.AddDefaultPolicy(policy =>
            {
                policy
                    .WithOrigins(allowedOrigins)
                    .AllowAnyMethod()
                    .AllowAnyHeader()
                    .AllowCredentials();
            });
        });

        return services;
    }
}