using AiChat.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace AiChat.Api.Data;

public sealed class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
    : DbContext(options)
{
    public DbSet<Conversation> Conversations => Set<Conversation>();
    public DbSet<Message> Messages => Set<Message>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Conversation>(entity =>
        {
            entity.Property(item => item.Title).HasMaxLength(200).IsRequired();
            entity.HasMany(item => item.Messages)
                .WithOne(item => item.Conversation)
                .HasForeignKey(item => item.ConversationId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<Message>(entity =>
        {
            entity.Property(item => item.Role).HasMaxLength(20).IsRequired();
            entity.Property(item => item.Content).IsRequired();
            entity.HasIndex(item => new { item.ConversationId, item.CreatedAt });
        });
    }
}
