using GoEStores.Repositories.Entity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GoEStores.Repositories.ConfigContext
{
    public static class EntityConfig
    {
        public static void ApplyAll(ModelBuilder modelBuilder)
        {
            new ChatHubConfig().Configure(modelBuilder.Entity<ChatHub>());
            new ChatMessageConfig().Configure(modelBuilder.Entity<ChatMessage>());
        }

        // ---------- CHATHUB ----------
        private class ChatHubConfig : IEntityTypeConfiguration<ChatHub>
        {
            public void Configure(EntityTypeBuilder<ChatHub> builder)
            {
                builder.HasKey(ch => ch.Id);

                builder.HasOne(ch => ch.FUser)
                       .WithMany(u => u.ChatHubsAsFUser)
                       .HasForeignKey(ch => ch.FUserId)
                       .OnDelete(DeleteBehavior.SetNull);

                builder.HasOne(ch => ch.SUser)
                       .WithMany(u => u.ChatHubsAsSUser)
                       .HasForeignKey(ch => ch.SUserId)
                       .OnDelete(DeleteBehavior.Restrict);
            }
        }

        // ---------- CHAT MESSAGE ----------
        private class ChatMessageConfig : IEntityTypeConfiguration<ChatMessage>
        {
            public void Configure(EntityTypeBuilder<ChatMessage> builder)
            {
                builder.HasKey(m => m.Id);

                builder.HasOne(m => m.ChatHub)
                       .WithMany(ch => ch.ChatMessages)
                       .HasForeignKey(m => m.ChatHubId)
                       .OnDelete(DeleteBehavior.Restrict);

                builder.HasOne(m => m.Sender)
                       .WithMany(u => u.SentMessages)
                       .HasForeignKey(m => m.SenderId)
                       .OnDelete(DeleteBehavior.SetNull);
            }
        }

    }
}
