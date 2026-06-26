using Nillero.Core.Application.Dtos.Social;
using Nillero.Core.Application.Interfaces.Base;

namespace Nillero.Core.Application.Interfaces.Social
{
    public interface ICommentService : IGenericService<CommentDto>
    {
        Task<List<CommentDto>> GetCommentsByPostIdAsync(int postId);
    }
}
