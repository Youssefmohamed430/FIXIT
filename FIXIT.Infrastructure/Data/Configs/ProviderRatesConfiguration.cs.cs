using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FIXIT.Infrastructure.Data.Configs;

public class ProviderRatesConfiguration : IEntityTypeConfiguration<ProviderRates>
{
    public void Configure(EntityTypeBuilder<ProviderRates> builder)
    {
        builder.HasKey(p => p.Id);

        builder.HasOne(p => p.Provider)
               .WithMany(sp => sp.Rates)
               .HasForeignKey(p => p.ProviderId)
               .IsRequired();

        builder.OwnsOne(j => j.Rate, rate =>
        {
            rate.Property(i => i.Value)
               .HasColumnName("Rate")
               .IsRequired();
        });
    }
}
