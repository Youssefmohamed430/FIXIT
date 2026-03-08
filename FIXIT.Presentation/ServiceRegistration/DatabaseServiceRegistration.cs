namespace FIXIT.Presentation.ServiceRegistration;

public static class DatabaseServiceRegistration
{
    public static IServiceCollection AddDatabaseServices(this IServiceCollection services, string connectionString)
    {
        services.AddDbContext<AppDbContext>(options =>
        {
            options.UseSqlServer(connectionString,
                x => x.UseNetTopologySuite());
        });

        return services;
    }
}
