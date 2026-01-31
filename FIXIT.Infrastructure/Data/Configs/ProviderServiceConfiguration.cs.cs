using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FIXIT.Infrastructure.Data.Configs
{
    public class ProviderServiceConfiguration : IEntityTypeConfiguration<ServiceProvider>
    {
        public void Configure(EntityTypeBuilder<ServiceProvider> builder)
        {
            builder.HasKey(s => s.Id);

            builder.HasOne(s => s.User)
                   .WithOne(u => u.ServiceProvider)
                   .HasForeignKey<ServiceProvider>(s => s.Id);
        }
    }
}
