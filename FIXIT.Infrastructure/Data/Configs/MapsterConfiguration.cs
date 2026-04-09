using FIXIT.Application.DTOs;
using FIXIT.Domain.ValueObjects;
using Mapster;
using Microsoft.Extensions.DependencyInjection;
using NetTopologySuite.Geometries;
using System.Reflection;
using ServiceProvider = FIXIT.Domain.Entities.ServiceProvider;

namespace FIXIT.Infrastructure.Data.Configs;

public static class MapsterConfiguration
{
    public static void RegisterMapsterConfiguration(this IServiceCollection services)
    {
        TypeAdapterConfig<JobPost, JobPostDTO>
            .NewConfig()
            .Map(dest => dest.JobPostImgPaths,
                 src => src.JobPostImgs
                           .Select(img => img.ImgPath.Value)
                           .ToList())
            .Map(dest => dest.CustomerName,
                 src => src.Customer.User.Name);
            //.Map(dest => dest.Location,
            //     src => src.Customer.User.Location);

        TypeAdapterConfig<CreateJobPostDTO, JobPost>
            .NewConfig()
            .Ignore(dest => dest.JobPostImgs)
            .Ignore(dest => dest.CreatedAt)
            .Ignore(dest => dest.Status);

        TypeAdapterConfig<CreateOrderDTO, Order>
            .NewConfig()
            .Ignore(dest => dest.CreatedAt)
            .Ignore(dest => dest.PaymentStatus)
            .Ignore(dest => dest.WorkStatus)
            .Ignore(dest => dest.PlatformCommission)
            .Ignore(dest => dest.ProviderAmount)
            .Ignore(dest => dest.TotalAmount);

        TypeAdapterConfig<Order, OrderDTO>
        .NewConfig()
        .Map(dest => dest.TotalAmount,
             src => src.TotalAmount.Amount)
        .Map(dest => dest.ProviderAmount,
             src => src.ProviderAmount.Amount)
        .Map(dest => dest.PlatformCommission,
             src => src.PlatformCommission.Amount);

        TypeAdapterConfig<OrderDTO, Order>
        .NewConfig()
        .Map(dest => dest.TotalAmount.Amount,
             src => src.TotalAmount)
        .Map(dest => dest.ProviderAmount.Amount,
             src => src.ProviderAmount)
        .Map(dest => dest.PlatformCommission.Amount,
             src => src.PlatformCommission);

        TypeAdapterConfig<Offer, OfferDTO>
        .NewConfig()
        .Map(dest => dest.Price,
             src => src.Price.Amount)
        .Map(dest => dest.ProviderName,
             src => src.ServiceProvider.User.Name);

        TypeAdapterConfig<CreateOfferDTO, Offer>
        .NewConfig()
        .Map(dest => dest.Price,
             src => src.Price != null ? Price.Create(src.Price.Value,"EGP") : null)
        .Ignore(dest => dest.CreatedAt)
        .Ignore(dest => dest.status)
        .Ignore(dest => dest.ServiceProvider)
        .Ignore(dest => dest.JobPost)
        .Ignore(dest => dest.orders);

        TypeAdapterConfig<decimal, Price>
        .NewConfig()
        .MapWith(src => Price.Create(src, "EGP"));

        TypeAdapterConfig<ImgPath, string>
            .NewConfig()
            .MapWith(src => src.Value);

        TypeAdapterConfig<Price, decimal>
            .NewConfig()
            .MapWith(src => src.Amount);

        TypeAdapterConfig<Price, string>
            .NewConfig()
            .MapWith(src => src.Currency);

        // FROM ApplicationUser TO RegisterDTO
        TypeAdapterConfig<ApplicationUser, RegisterDTO>
            .NewConfig()
            .Map(dest => dest.Longitude,
                 src => src.Location != null ? src.Location.X : 0)
            .Map(dest => dest.Latitude,
                 src => src.Location != null ? src.Location.Y : 0);

        // FROM RegisterDTO TO ApplicationUser
        TypeAdapterConfig<RegisterDTO, ApplicationUser>
            .NewConfig()
            .ConstructUsing(src => new ApplicationUser
            {
                Name = src.Name!,
                UserName = src.UserName!,
                Email = src.Email!,
                PhoneNumber = src.Phone!,
                Location = new Point(src.Longitude, src.Latitude) { SRID = 4326 }
            })
            .IgnoreNonMapped(true);

        TypeAdapterConfig<UserNotification, NotifDTO>
            .NewConfig()
            .Map(dest => dest.Message, src => src.Notif.Message);

        TypeAdapterConfig<Wallet, WalletDTO>
            .NewConfig()
            .Map(dest => dest.Name, src => src.User.Name)
            .Map(dest => dest.email, src => src.User.Email);

        TypeAdapterConfig<Chat, ChatDTO>
            .NewConfig()
            .Map(dest => dest.ChatId, src => src.Id)
            .Map(dest => dest.Participants, src => src.Participants)
            .Map(dest => dest.Messages, src => src.Messages);

        TypeAdapterConfig<ChatParticipant, ChatUserDto>
            .NewConfig()
            .Map(dest => dest.UserId, src => src.UserId)
            .Map(dest => dest.Name, src => src.User.Name)
            .Map(dest => dest.ImageUrl,
                 src => src.User != null && src.User.Img != null
                        ? src.User.Img.Value  
                        : null);

        TypeAdapterConfig<ChatMessage, MessageDto>
            .NewConfig()
            .Map(dest => dest.Message, src => src.Message);

        TypeAdapterConfig<ProviderRates, ProviderRatingDTO>
        .NewConfig()
        .Map(dest => dest.ProviderName, src => src.Provider.User.Name)
        .Map(dest => dest.CustomerName, src => src.Customer.User.Name)
        .Map(dest => dest.Rate, src => src.Rate != null ? src.Rate.Value : 0);

        TypeAdapterConfig<ProviderRatingDTO, ProviderRates>
            .NewConfig()
            .Map(dest => dest.Rate, src => Rate.Create(src.Rate))
            .Map(dest => dest.ProviderId, src => src.ProviderId)
            .Map(dest => dest.CustomerId, src => src.CustomerID);


        TypeAdapterConfig.GlobalSettings.Scan(Assembly.GetExecutingAssembly());
    }
}
