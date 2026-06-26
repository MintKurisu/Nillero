using Nillero.Core.Domain.Entities.Social;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Nillero.Infrastructure.Persistence.EntityConfigurations.Social
{
    public class PostReactionEntityConfiguration : IEntityTypeConfiguration<PostReaction>
    {
        public void Configure(EntityTypeBuilder<PostReaction> builder)
        {
            builder.HasKey(x => x.Id);
            builder.ToTable("PostReactions");

            builder.Property(x => x.Type).IsRequired();
            builder.Property(x => x.CreatedAt).IsRequired();

            builder.HasIndex(x => new { x.PostId, x.UserId }).IsUnique();

            // FK a Identity_Users
            builder.Property(x => x.UserId).IsRequired().HasMaxLength(255);
            builder.HasIndex(x => x.UserId);

            // Relaciones
            builder.HasOne(x => x.Post)
                .WithMany(p => p.Reactions)
                .HasForeignKey(x => x.PostId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
