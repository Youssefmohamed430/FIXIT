using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FIXIT.Infrastructure.Data.Configs
{
    public class WalletTransactionConfiguration : IEntityTypeConfiguration<WalletTransaction>
    {
        public void Configure(EntityTypeBuilder<WalletTransaction> builder)
        {
            builder.HasKey
                (wt => wt.Id);

            builder.OwnsOne(o => o.Amount, priceBuilder =>
            {
                priceBuilder.Property(p => p.Amount)
                            .HasColumnName("Price_Amount")
                            .IsRequired();
                priceBuilder.Property(p => p.Currency)
                            .HasColumnName("Price_Currency")
                            .IsRequired();
            });

            builder.HasOne(wt => wt.Wallet)
                   .WithMany(w => w.WalletTransactions)
                   .HasForeignKey(wt => wt.WalletId)
                   .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(wt => wt.Order)
                     .WithMany(o => o.walletTransactions)
                     .HasForeignKey(wt => wt.OrderId)
                     .OnDelete(DeleteBehavior.Restrict);

        }
    }
}
