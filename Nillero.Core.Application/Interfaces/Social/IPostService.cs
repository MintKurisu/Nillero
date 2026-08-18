using Nillero.Core.Application.Dtos.Social;
using Nillero.Core.Application.Interfaces.Base;

namespace Nillero.Core.Application.Interfaces.Social
{
    public interface IPostService : IGenericService<PostDto>
    {
        Task<List<PostDto>> GetPostsByUserIdAsync(string userId);
        Task<List<PostDto>> GetFriendsPostsAsync(string userId);
        Task<PostDto?> GetPostWithDetailsAsync(int postId);
        Task<List<PostDto>> SearchPostsAsync(string searchTerm, int maxResults = 4);

    }
}
