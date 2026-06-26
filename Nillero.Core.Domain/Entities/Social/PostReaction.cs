using Nillero.Core.Domain.Common.Enum.Social;

namespace Nillero.Core.Domain.Entities.Social
{
    public class PostReaction
    {
        public int Id { get; set; }
        public required int PostId { get; set; }
        public required string UserId { get; set; }
        public ReactionType Type { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation Properties
        public Post Post { get; set; } = null!;

    }
}
