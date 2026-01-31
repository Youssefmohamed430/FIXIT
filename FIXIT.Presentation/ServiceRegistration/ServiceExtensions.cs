using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FIXIT.Presentation.ServiceRegistration
{
    public static class ServiceExtensions
    {
        public static IServiceCollection AddApplicationServices(this IServiceCollection services, IConfiguration config)
        {
            var appsettingsconfig = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json")
                .Build();

            services.AddDataBaseConfiguration(appsettingsconfig);
            //services.AddAppConfigurations(config);
            //services.AddSerilogConfigs(appsettingsconfig);
            //services.AddIdentityService();
            //services.AddJwtAuthentication(config);
            //services.AddService();
            //services.AddCorsPolicy();
            //services.AddRateLimiter();
            //services.AddRemainingAppConfigs();


            return services;
        }
    }
}
