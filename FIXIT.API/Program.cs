using DotNetEnv;
using FIXIT.Application.IServices;
using FIXIT.Application.Servicces;
using FIXIT.Domain.Abstractions;
using FIXIT.Domain.Entities;
using FIXIT.Domain.Helpers;
using FIXIT.Domain.LocalizationsFiles;
using FIXIT.Infrastructure;
using FIXIT.Infrastructure.Data.Configs;
using FIXIT.Infrastructure.Data.Context;
using FIXIT.Presentation;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Localization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Localization;
using Microsoft.IdentityModel.Tokens;
using System.Globalization;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

Env.Load();

var config = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json")
                .Build();

var connectionString =
    Environment.GetEnvironmentVariable("constr");

builder.Services.AddScoped<HandleCachingResourcesFilter>();

builder.Services.AddControllers(options =>
{
    options.Filters.Add<HandleCachingResourcesFilter>();
});


builder.Services.AddDbContext<AppDbContext>(options =>
{
    options.UseSqlServer(connectionString,
        x => x.UseNetTopologySuite());
});
builder.Services.AddTransient<IUnitOfWork, UnitOfWork>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IWallettService, WalletService>();
builder.Services.AddScoped<IEmailService, EmailService>();
builder.Services.AddScoped<IServiceManager, ServiceManager>();
builder.Services.AddScoped<JWTService>();
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
}).AddJwtBearer(o =>
{
    o.RequireHttpsMetadata = false;
    o.SaveToken = false;
    o.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidIssuer = config["JWT:Issuer"],
        ValidAudience = config["JWT:Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8
        .GetBytes(Environment.GetEnvironmentVariable("JWTKey")!)),
        ClockSkew = TimeSpan.Zero
    };
});

builder.Services.AddAuthentication()
                .AddGoogle(options =>
                {
                    options.ClientId = Environment.GetEnvironmentVariable("ClientGoogleId")!;
                    options.ClientSecret = Environment.GetEnvironmentVariable("ClientGoogleSecret")!;
                });

builder.Services.AddOpenApi();
builder.Services.AddMemoryCache();

builder.Services.AddLocalization();
builder.Services.AddSingleton<IStringLocalizerFactory, JsonStringLocalizerFactory>();


builder.Services.Configure<RequestLocalizationOptions>(options =>
{
    var supportedCultures = new[]
    {
        new CultureInfo("en-US"),
        new CultureInfo("ar-EG"),
    };

    options.DefaultRequestCulture = new RequestCulture(culture: supportedCultures[0]);
    options.SupportedCultures = supportedCultures;
});

builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
{
    options.SignIn.RequireConfirmedAccount = true; 
    options.Password.RequireDigit = true;
    options.Password.RequiredLength = 8;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequireUppercase = true;
    options.Password.RequireLowercase = true;
}).AddEntityFrameworkStores<AppDbContext>()
         .AddDefaultTokenProviders();

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

app.UseAuthentication();

app.UseAuthorization();

app.MapControllers();

app.Run();
