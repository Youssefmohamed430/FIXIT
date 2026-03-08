
using System.Text.Json.Serialization;

namespace FIXIT.Presentation.ServiceRegistration;

public static class ControllerServiceRegistration
{
    public static IServiceCollection AddControllerServices(this IServiceCollection services)
    {
        services.AddScoped<HandleCachingResourcesFilter>();

        services.AddControllers(options =>
        {
            options.Filters.Add<HandleCachingResourcesFilter>();
        })
        .AddJsonOptions(options =>
        {
            options.JsonSerializerOptions.Converters
                .Add(new JsonStringEnumConverter());
        });

        return services;
    }
}
