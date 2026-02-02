using FIXIT.Domain.Entities;
using FIXIT.Infrastructure.Data.Context;
using FIXIT.Presentation.ServiceRegistration;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

var connectionString =
    builder.Configuration.GetConnectionString("constr");

builder.Services.AddDbContext<AppDbContext>(options =>
{
    options.UseSqlServer(connectionString,
        x => x.UseNetTopologySuite());
});

builder.Services.AddControllers();
builder.Services.AddOpenApi();

//builder.Services.AddApplicationServices(builder.Configuration);

builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
{
    options.SignIn.RequireConfirmedAccount = true; // Add this line
    options.Password.RequireDigit = true;
    options.Password.RequiredLength = 8;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequireUppercase = true;
    options.Password.RequireLowercase = true;
}).AddEntityFrameworkStores<AppDbContext>()
         .AddDefaultTokenProviders();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
