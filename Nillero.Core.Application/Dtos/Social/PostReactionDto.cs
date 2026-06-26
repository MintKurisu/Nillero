using Nillero.Core.Domain.Common.Enum.Social;

namespace Nillero.Core.Application.Dtos.Social
{
    public class PostReactionDto
    {
        public int Id { get; set; }
        public required int PostId { get; set; }
        public required string UserId { get; set; }
        public required ReactionType Type { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
