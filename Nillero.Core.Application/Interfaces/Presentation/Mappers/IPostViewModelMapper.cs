using Nillero.Core.Application.Dtos.Social;
using Nillero.Core.Application.ViewModels.Social.Posts;

namespace Nillero.Core.Application.Interfaces.Presentation.Mappers
{
    public interface IPostViewModelMapper
    {
        Task<List<PostViewModel>> MapAsync(
            List<PostDto> posts,
            string currentUserId);
    }
}
