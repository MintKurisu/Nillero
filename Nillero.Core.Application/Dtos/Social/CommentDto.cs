namespace Nillero.Core.Application.Dtos.Social
{
    public class CommentDto
    {
        public int Id { get; set; }
        public required int PostId { get; set; }
        public required string UserId { get; set; }
        public int? ParentCommentId { get; set; }
        public int? RootCommentId { get; set; } 
        public required string Content { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public List<CommentDto> Replies { get; set; } = new(); 

    }
}
