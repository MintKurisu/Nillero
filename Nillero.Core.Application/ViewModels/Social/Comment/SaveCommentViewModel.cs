using System.ComponentModel.DataAnnotations;

namespace Nillero.Core.Application.ViewModels.Social.Comment
{
    public class SaveCommentViewModel
    {
        public int? Id { get; set; }
        public required int PostId { get; set; }
        public int? ParentCommentId { get; set; }
        public int? RootCommentId { get; set; } 

        [Required(ErrorMessage = "Content is required.")]
        [StringLength(2000)]
        [Display(Name = "Comment")]
        public required string Content { get; set; }
    }
}
