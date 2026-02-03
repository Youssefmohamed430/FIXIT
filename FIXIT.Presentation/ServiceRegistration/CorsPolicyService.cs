using Microsoft.Extensions.DependencyInjection;

namespace Service_Layer.ServiceRegistration;

public static class CorsPolicyService
{
    public static IServiceCollection AddCorsPolicy(this IServiceCollection services)
    {
        //services.AddCors(corsOptions =>
        //      corsOptions.AddPolicy("MyPolicy", CorsPolicy =>
        //          CorsPolicy.WithOrigins(
        //                "https://youssefmohamed430.github.io",
        //                "http://localhost:5173"
        //            )
        //            .AllowAnyHeader()
        //            .AllowAnyMethod()
        //            .AllowCredentials()
        //));

        return services;
    }
}
