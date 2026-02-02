using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FIXIT.Infrastructure.Data.Configs;

public class OfferConfiguration : IEntityTypeConfiguration<Offer>
{
    public void Configure(EntityTypeBuilder<Offer> builder)
    {
        builder.HasKey(o => o.Id);

        builder.OwnsOne(o => o.Price, priceBuilder =>
        {
            priceBuilder.Property(p => p.Amount)
                        .HasColumnName("Price_Amount")
                        .IsRequired();
            priceBuilder.Property(p => p.Currency)
                        .HasColumnName("Price_Currency")
                        .IsRequired();
        });

        builder.HasOne(o => o.ServiceProvider)
               .WithMany(sp => sp.Offers)
               .HasForeignKey(o => o.ProviderId)
               .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(o => o.JobPost)
                .WithMany(jp => jp.Offers)
                .HasForeignKey(o => o.JobPostId)
                .OnDelete(DeleteBehavior.Restrict);

    }
}
