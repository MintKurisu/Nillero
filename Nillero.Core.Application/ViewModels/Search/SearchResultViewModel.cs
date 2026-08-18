using Nillero.Core.Application.ViewModels.Social.Posts;

namespace Nillero.Core.Application.ViewModels.Search
{
    public class SearchResultViewModel
    {
        public string Query { get; set; } = string.Empty;

        // People results
        public List<UserSearchItemViewModel> People { get; set; } = new();

        // Metadata used for the live dropdown snippet + author info
        public List<PostSearchItemViewModel> Posts { get; set; } = new();

        // Full PostViewModels for _PostCard partial in the results page
        // Populated only by SearchController.Index(), not by the Live endpoint.
        public List<PostViewModel> PostViewModels { get; set; } = new();

        public bool HasResults => People.Any() || Posts.Any();
    }
}
