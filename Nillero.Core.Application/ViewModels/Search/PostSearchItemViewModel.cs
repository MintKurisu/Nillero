namespace Nillero.Core.Application.ViewModels.Search
{
    public class PostSearchItemViewModel
    {
        public int PostId { get; set; }
        public string AuthorUserName { get; set; } = string.Empty;
        public string AuthorFullName { get; set; } = string.Empty;
        public string? AuthorProfilePicture { get; set; }
        // ~120-char excerpt centered around the matched term
        public string ContentSnippet { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
    }
}
