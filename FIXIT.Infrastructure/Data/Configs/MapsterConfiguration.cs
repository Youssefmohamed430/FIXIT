using FIXIT.Application.DTOs;
using FIXIT.Application.DTOsك;
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

        TypeAdapterConfig<CreateJobPostDTO, JobPost>
            .NewConfig()
            .Ignore(dest => dest.JobPostImgs);

        TypeAdapterConfig<ImgPath, string>
            .NewConfig()
            .MapWith(src => src.Value);

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

        TypeAdapterConfig.GlobalSettings.Scan(Assembly.GetExecutingAssembly());
    }
}
