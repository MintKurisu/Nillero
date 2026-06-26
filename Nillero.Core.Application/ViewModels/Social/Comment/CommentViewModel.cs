namespace Nillero.Core.Application.ViewModels.Social.Comment
{
    public class CommentViewModel
    {
        public int Id { get; set; }
        public required int PostId { get; set; }
        public required string UserId { get; set; }
        public required string UserName { get; set; }
        public required string UserFullName { get; set; }
        public string? UserProfilePicture { get; set; }
        public int? ParentCommentId { get; set; }
        public required string Content { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public bool IsOwner { get; set; }
        public List<CommentViewModel> Replies { get; set; } = new();
    }
}
