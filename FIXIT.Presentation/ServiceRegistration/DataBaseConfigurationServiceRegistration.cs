using Microsoft.EntityFrameworkCore;
using NetTopologySuite;
using NetTopologySuite.Geometries;

namespace FIXIT.Presentation.ServiceRegistration;

public static class DataBaseConfigurationServiceRegistration
{
    public static IServiceCollection AddDataBaseConfiguration(this IServiceCollection services, IConfigurationRoot config)
    {
        //var Connectionstring = Environment.GetEnvironmentVariable("Constr");
        //var Connectionstring = config.GetConnectionString("constr");

        //services.AddDbContextPool<AppDbContext>(options =>
        //{
        //    options.UseSqlServer(Connectionstring,
        //    x => x.UseNetTopologySuite());
        //});

        return services;
    }
}
