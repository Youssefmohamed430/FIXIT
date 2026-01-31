using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FIXIT.Infrastructure.Data.Configs;

public class JobPostConfiguration : IEntityTypeConfiguration<JobPost>
{
    public void Configure(EntityTypeBuilder<JobPost> builder)
    {
        builder.HasKey(Jp => Jp.Id);

        builder.Property(Jp => Jp.Description)
            .IsRequired()
            .HasMaxLength(1000);

        builder.Property(Jp => Jp.ServiceType)
            .IsRequired()
            .HasMaxLength(100);

        builder.HasOne(Jp => Jp.Customer)
            .WithMany(C => C.posts)
            .HasForeignKey(Jp => Jp.CustomerId);
    }
}
