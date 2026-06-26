using Nillero.Core.Application.ViewModels.Social.Comment;
using Nillero.Core.Domain.Common.Enum.Social;

namespace Nillero.Core.Application.ViewModels.Social.Posts
{
    public class PostViewModel
    {
        public int Id { get; set; }
        public required string UserId { get; set; }
        public required string UserName { get; set; }
        public required string UserFullName { get; set; }
        public string? UserProfilePicture { get; set; }
        public required string Content { get; set; }
        public required PostType Type { get; set; }
        public string? MediaPath { get; set; }
        public string? YouTubeUrl { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public bool IsOwner { get; set; }
        public int LikeCount { get; set; }
        public int DislikeCount { get; set; }
        public ReactionType? UserReaction { get; set; }
        public List<CommentViewModel> Comments { get; set; } = new();
    }
}
