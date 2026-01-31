using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FIXIT.Infrastructure.Data.Configs;

public class WalletConfiguration : IEntityTypeConfiguration<Wallet>
{
    public void Configure(EntityTypeBuilder<Wallet> builder)
    {
        builder.HasKey(w => w.Id);  

        builder.Property(w => w.UserId).IsRequired();

        builder.OwnsOne(o => o.Balance, priceBuilder =>
        {
            priceBuilder.Property(p => p.Amount)
                        .HasColumnName("Price_Amount")
                        .IsRequired();
            priceBuilder.Property(p => p.Currency)
                        .HasColumnName("Price_Currency")
                        .IsRequired();
        });
    }
}
