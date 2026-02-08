using FIXIT.Application.DTOs;
using Mapster;
using Microsoft.Extensions.DependencyInjection;
using NetTopologySuite.Geometries;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace FIXIT.Infrastructure.Data.Configs
{
    public static class MapsterConfiguration
    {
        public static void RegisterMapsterConfiguration(this IServiceCollection services)
        {
            TypeAdapterConfig<ApplicationUser, RegisterDTO>
            .NewConfig()
            .Map(dest => dest.Longitude,
                 src => src.Location != null ? src.Location.X : 0)
            .Map(dest => dest.Latitude,
                 src => src.Location != null ? src.Location.Y : 0);

            TypeAdapterConfig<RegisterDTO, ApplicationUser>
            .NewConfig()
            .Map(dest => dest.Location,
                 src => new Point(src.Longitude, src.Latitude) { SRID = 4326 });

            TypeAdapterConfig.GlobalSettings.Scan(Assembly.GetExecutingAssembly());
        }
    }
}
