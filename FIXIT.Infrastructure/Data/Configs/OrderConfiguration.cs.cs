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
                            .HasColumnName("Price_Amount")
                            .IsRequired();
                priceBuilder.Property(p => p.Currency)
                            .HasColumnName("Price_Currency")
                            .IsRequired();
            });

            builder.OwnsOne(o => o.ProviderAmount, priceBuilder =>
            {
                priceBuilder.Property(p => p.Amount)
                            .HasColumnName("Price_Amount")
                            .IsRequired();
                priceBuilder.Property(p => p.Currency)
                            .HasColumnName("Price_Currency")
                            .IsRequired();
            });

            builder.OwnsOne(o => o.PlatformCommission, priceBuilder =>
            {
                priceBuilder.Property(p => p.Amount)
                            .HasColumnName("Price_Amount")
                            .IsRequired();
                priceBuilder.Property(p => p.Currency)
                            .HasColumnName("Price_Currency")
                            .IsRequired();
            });

            builder.HasOne(o => o.JobPost)
                   .WithMany(jp => jp.orders)
                   .HasForeignKey(o => o.JobPostId);

            builder.HasOne(o => o.Offer)
                     .WithMany(of => of.orders)
                     .HasForeignKey(o => o.OfferId);


        }
    }
}
