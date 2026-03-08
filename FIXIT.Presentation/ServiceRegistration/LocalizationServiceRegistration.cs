using FIXIT.Domain.LocalizationsFiles;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Localization;
using Microsoft.Extensions.Localization;
using System.Globalization;

namespace FIXIT.Presentation.ServiceRegistration;

public static class LocalizationServiceRegistration
{
    public static IServiceCollection AddLocalizationServices(this IServiceCollection services)
    {
        services.AddLocalization();

        services.AddSingleton<IStringLocalizerFactory, JsonStringLocalizerFactory>();

        services.Configure<RequestLocalizationOptions>(options =>
        {
            var supportedCultures = new[]
            {
                new CultureInfo("en-US"),
                new CultureInfo("ar-EG"),
            };

            options.DefaultRequestCulture = new RequestCulture(culture: supportedCultures[0]);
            options.SupportedCultures = supportedCultures;
        });

        return services;
    }
}
