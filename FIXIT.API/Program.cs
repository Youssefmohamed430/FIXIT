


var builder = WebApplication.CreateBuilder(args);

Env.Load();

var config = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json")
                .Build();

var connectionString =
    Environment.GetEnvironmentVariable("constr");

builder.Services.AddScoped<HandleCachingResourcesFilter>();
builder.Services.AddScoped<IdempotencyKeyFilter>();


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

var supportedCultures = new[] { "en-US", "ar-EG"};

var localizationOptions = new RequestLocalizationOptions()
    .SetDefaultCulture(supportedCultures[0])
    .AddSupportedCultures(supportedCultures)
    .AddSupportedUICultures(supportedCultures);

app.UseRequestLocalization(localizationOptions);

app.UseCors();

app.UseAuthentication();

app.UseAuthorization();

app.MapControllers();

app.Run();
