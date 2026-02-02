using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FIXIT.Infrastructure.Data.Configs
{
    public class OrderConfiguration : IEntityTypeConfiguration<Order>
    {
        public void Configure(EntityTypeBuilder<Order> builder)
        {
            builder.HasKey(o => o.Id);

            builder.OwnsOne(o => o.TotalAmount, priceBuilder =>
            {
                priceBuilder.Property(p => p.Amount)
                            .HasColumnName("TotalAmount_Amount");  
                priceBuilder.Property(p => p.Currency)
                            .HasColumnName("TotalAmount_Currency");  
            });

            builder.OwnsOne(o => o.ProviderAmount, priceBuilder =>
            {
                priceBuilder.Property(p => p.Amount)
                            .HasColumnName("ProviderAmount_Amount");
                priceBuilder.Property(p => p.Currency)
                            .HasColumnName("ProviderAmount_Currency");
            });

            builder.OwnsOne(o => o.PlatformCommission, priceBuilder =>
            {
                priceBuilder.Property(p => p.Amount)
                            .HasColumnName("PlatformCommission_Amount");
                priceBuilder.Property(p => p.Currency)
                            .HasColumnName("PlatformCommission_Currency");
            });

            builder.HasOne(o => o.JobPost)
                   .WithMany(jp => jp.orders)
                   .HasForeignKey(o => o.JobPostId)
                   .OnDelete(DeleteBehavior.Restrict);  

            builder.HasOne(o => o.Offer)
                     .WithMany(of => of.orders)
                     .HasForeignKey(o => o.OfferId)
                     .OnDelete(DeleteBehavior.Restrict);


        }
    }
}
