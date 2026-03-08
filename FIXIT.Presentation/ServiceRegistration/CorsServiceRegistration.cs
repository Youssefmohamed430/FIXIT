namespace FIXIT.Presentation.ServiceRegistration;

public static class CorsServiceRegistration
{
    public static IServiceCollection AddCorsServices(this IServiceCollection services)
    {
        services.AddCors(options =>
        {
            options.AddDefaultPolicy(policy =>
            {
                policy.AllowAnyOrigin()
                      .AllowAnyMethod()
                      .AllowAnyHeader()
                      .WithOrigins("https://localhost:7083")
                      .AllowCredentials();
            });
        });

        return services;
    }
}