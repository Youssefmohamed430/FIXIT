using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FIXIT.Infrastructure.Data.Configs
{
    public class ChatParticipantConfiguration : IEntityTypeConfiguration<ChatParticipant>
    {
        public void Configure(EntityTypeBuilder<ChatParticipant> builder)
        {
            builder.HasKey(cp => cp.Id);

            builder.HasOne(cp => cp.Chat)
                   .WithMany(c => c.Participants)
                   .HasForeignKey(cp => cp.ChatId);

            builder.HasOne(cp => cp.User)
                     .WithMany(u => u.ChatParticipants)
                     .HasForeignKey(cp => cp.UserId);
        }
    }
}
