using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FIXIT.Infrastructure.Data.Configs;

public class JobPostImgConfiguration : IEntityTypeConfiguration<JobPostImg>
{
    public void Configure(EntityTypeBuilder<JobPostImg> builder)
    {
        builder.HasKey(Jpi => Jpi.Id);

        builder.OwnsOne(j => j.ImgPath, img =>
        {
            img.Property(i => i.Value)
               .HasColumnName("ImagePath")
               .IsRequired();
        });


        builder.HasOne(Jpi => Jpi.JobPost)
               .WithMany(jp => jp.JobPostImgs)
               .HasForeignKey(jp => jp.JobPostId);
    }
}
