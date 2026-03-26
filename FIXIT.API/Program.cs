try
{
    Log.Logger.Information("Starting FIXIT API...");

    var builder = WebApplication.CreateBuilder(args);

    Env.Load();

    var config = new ConfigurationBuilder()
                    .AddJsonFile("appsettings.json")
                    .Build();

    var connectionString =
        Environment.GetEnvironmentVariable("constr");

    builder.Services.AddScoped<HandleCachingResourcesFilter>();
    builder.Services.AddScoped<IdempotencyKeyFilter>();

    builder.Host.UseSerilog((context, services, configuration) =>
            configuration.ReadFrom.Configuration(context.Configuration)
                         .Enrich.FromLogContext()
                         .WriteTo.Console());

    Log.Logger = new LoggerConfiguration()
        .MinimumLevel.Information()
        .WriteTo.Console()
        .WriteTo.File(
            path: "logs/log-.txt",
            rollingInterval: RollingInterval.Day,
            retainedFileCountLimit: 7,
            fileSizeLimitBytes: 10_000_000,
            rollOnFileSizeLimit: true
        )
        .CreateLogger();

    builder.Host.UseSerilog();

    builder.Services.AddCoreApplicationServices(config);


    builder.Services.AddOpenApi();
    builder.Services.AddMemoryCache();
    builder.Services.RegisterMapsterConfiguration();


    var app = builder.Build();

    if (app.Environment.IsDevelopment())
    {
        app.MapOpenApi();
    }

    app.UseHttpsRedirection();

    var supportedCultures = new[] { "en-US", "ar-EG" };

    var localizationOptions = new RequestLocalizationOptions()
        .SetDefaultCulture(supportedCultures[0])
        .AddSupportedCultures(supportedCultures)
        .AddSupportedUICultures(supportedCultures);

    app.UseRequestLocalization(localizationOptions);

    app.UseCors();

    app.UseAuthentication();

    app.UseAuthorization();

    app.MapControllers();

    app.UseSerilogRequestLogging();

    app.Run();
    Log.Logger.Information("FIXIT API started successfully.");
}
catch (Exception ex)
{
    Log.Logger.Fatal(ex, "An unhandled exception occurred while starting the FIXIT API.");
}
finally
{
    Log.CloseAndFlush();
}
