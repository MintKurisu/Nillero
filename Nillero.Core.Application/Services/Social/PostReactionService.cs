using Nillero.Core.Application.Interfaces.Social;
using Nillero.Core.Domain.Common.Enum.Social;
using Nillero.Core.Domain.Entities.Social;
using Nillero.Core.Domain.Interfaces.Social;
using Microsoft.EntityFrameworkCore;

namespace Nillero.Core.Application.Services.Social
{
    public class PostReactionService : IPostReactionService
    {
        private readonly IPostReactionRepository _reactionRepository;

        public PostReactionService(IPostReactionRepository reactionRepository)
        {
            _reactionRepository = reactionRepository;
        }

        public async Task<bool> ToggleReactionAsync(string userId, int postId, ReactionType type)
        {
            try
            {
                var query = _reactionRepository.GetAllQuery();
                var existingReaction = await query
                    .FirstOrDefaultAsync(r => r.UserId == userId && r.PostId == postId);

                if (existingReaction != null)
                {
                    existingReaction.Type = type;
                    await _reactionRepository.UpdateAsync(existingReaction.Id, existingReaction);
                    return true;
                }

                // create new reaction
                var newReaction = new PostReaction
                {
                    UserId = userId,
                    PostId = postId,
                    Type = type,
                    CreatedAt = DateTime.UtcNow
                };

                await _reactionRepository.AddAsync(newReaction);
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in ToggleReactionAsync: {ex.Message}");
                throw;
            }
        }

        public async Task<bool> RemoveReactionAsync(string userId, int postId)
        {
            try
            {
                var query = _reactionRepository.GetAllQuery();
                var reaction = await query
                    .FirstOrDefaultAsync(r => r.UserId == userId && r.PostId == postId);

                if (reaction == null)
                    return false;

                await _reactionRepository.DeleteAsync(reaction.Id);
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in RemoveReactionAsync: {ex.Message}");
                throw;
            }
        }

        public async Task<(int likes, int dislikes)> GetReactionCountsAsync(int postId)
        {
            try
            {
                var query = _reactionRepository.GetAllQuery();
                var reactions = await query.Where(r => r.PostId == postId).ToListAsync();

                var likes = reactions.Count(r => r.Type == ReactionType.Like);
                var dislikes = reactions.Count(r => r.Type == ReactionType.Dislike);

                return (likes, dislikes);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in GetReactionCountsAsync: {ex.Message}");
                throw;
            }
        }

        public async Task<ReactionType?> GetUserReactionAsync(string userId, int postId)
        {
            try
            {
                var query = _reactionRepository.GetAllQuery();
                var reaction = await query
                    .FirstOrDefaultAsync(r => r.UserId == userId && r.PostId == postId);

                return reaction?.Type;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in GetUserReactionAsync: {ex.Message}");
                throw;
            }
        }
    }
}
