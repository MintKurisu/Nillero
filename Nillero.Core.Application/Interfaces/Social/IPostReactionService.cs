using Nillero.Core.Domain.Common.Enum.Social;

namespace Nillero.Core.Application.Interfaces.Social
{
    public interface IPostReactionService
    {
        Task<bool> ToggleReactionAsync(string userId, int postId, ReactionType type);
        Task<bool> RemoveReactionAsync(string userId, int postId);
        Task<(int likes, int dislikes)> GetReactionCountsAsync(int postId);
        Task<ReactionType?> GetUserReactionAsync(string userId, int postId);
    }
}
