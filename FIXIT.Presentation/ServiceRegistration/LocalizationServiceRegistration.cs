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

        var supportedCultures = new[]
        {
            new CultureInfo("en-US"),
            new CultureInfo("ar-EG"),
        };

        services.Configure<RequestLocalizationOptions>(options =>
        {
            options.DefaultRequestCulture = new RequestCulture("en-US");
            options.SupportedCultures = supportedCultures;
            options.SupportedUICultures = supportedCultures;

            options.RequestCultureProviders = new List<IRequestCultureProvider>
            {
                new QueryStringRequestCultureProvider(),  // ?culture=ar-EG
                new CookieRequestCultureProvider(),
                new AcceptLanguageHeaderRequestCultureProvider() // Accept-Language: ar-EG
            };
        });

        return services;
    }
}