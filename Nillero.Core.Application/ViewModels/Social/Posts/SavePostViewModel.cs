using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace Nillero.Core.Application.ViewModels.Social.Posts
{
    public class SavePostViewModel
    {
        public int? Id { get; set; } // Null if new, has value if editing

        [Required(ErrorMessage = "Content is required.")]
        [StringLength(5000)]
        [Display(Name = "Content")]
        public required string Content { get; set; }

        [Required(ErrorMessage = "You must select the publication type.")]
        [Display(Name = "Content Type")]
        public required string MediaType { get; set; } // "Image" or "Video"

        [Display(Name = "Image")]
        public IFormFile? ImageFile { get; set; }

        [Url(ErrorMessage = "The YouTube URL is not valid.")]
        [Display(Name = "YouTube Link")]
        public string? YouTubeUrl { get; set; }
    }
}
