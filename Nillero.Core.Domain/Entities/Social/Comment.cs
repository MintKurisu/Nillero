using System.ComponentModel.DataAnnotations;

namespace Nillero.Core.Domain.Entities.Social
{
    public class Comment
    {
        public int Id { get; set; }
        public required int PostId { get; set; }
        public required string UserId { get; set; }
        public int? ParentCommentId { get; set; }
        public int? RootCommentId { get; set; } 

        [StringLength(2000)]
        public required string Content { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }

        // Navigation Properties
        public Post Post { get; set; } = null!;
        public Comment? ParentComment { get; set; }
        public Comment? RootComment { get; set; }
        public ICollection<Comment> Replies { get; set; } = new List<Comment>();
    }
}
