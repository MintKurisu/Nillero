using Nillero.Core.Domain.Common.Enum.Social;
using System.ComponentModel.DataAnnotations;

namespace Nillero.Core.Domain.Entities.Social
{
    public class Post
    {
        public int Id { get; set; }
        public required string UserId { get; set; }

        [StringLength(5000)]
        public required string Content { get; set; }
        public PostType Type { get; set; }

        [StringLength(500)]
        public string? MediaPath { get; set; } 

        [StringLength(500)]
        public string? YouTubeUrl { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }

        // Navigation Properties
        public ICollection<Comment> Comments { get; set; } = new List<Comment>();
        public ICollection<PostReaction> Reactions { get; set; } = new List<PostReaction>();
    }
}
