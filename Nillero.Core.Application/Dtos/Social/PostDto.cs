using Nillero.Core.Domain.Common.Enum.Social;

namespace Nillero.Core.Application.Dtos.Social
{
    public class PostDto
    {
        public int Id { get; set; }
        public required string UserId { get; set; }
        public required string Content { get; set; }
        public required PostType Type { get; set; }
        public string? MediaPath { get; set; } 
        public string? YouTubeUrl { get; set; } 
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; } 
    }
}
