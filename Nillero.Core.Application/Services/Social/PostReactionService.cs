using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Nillero.Core.Application.Dtos.Notifications;
using Nillero.Core.Application.Interfaces.Notifications;
using Nillero.Core.Application.Interfaces.Social;
using Nillero.Core.Domain.Common.Enum;
using Nillero.Core.Domain.Common.Enum.Social;
using Nillero.Core.Domain.Entities.Notifications;
using Nillero.Core.Domain.Entities.Social;
using Nillero.Core.Domain.Interfaces.Notifications;
using Nillero.Core.Domain.Interfaces.Social;

namespace Nillero.Core.Application.Services.Social
{
    public class PostReactionService : IPostReactionService
    {
        private readonly IPostReactionRepository _reactionRepository;
        private readonly IPostRepository _postRepository;
        private readonly INotificationRepository _notificationRepository;
        private readonly IMapper _mapper;
        private readonly IRealTimeNotificationService _rtNotificationService;

        public PostReactionService(
            IPostReactionRepository reactionRepository,
            IPostRepository postRepository,
            INotificationRepository notificationRepository,
            IMapper mapper,
            IRealTimeNotificationService rtNotificationService)
        {
            _reactionRepository = reactionRepository;
            _postRepository = postRepository;
            _notificationRepository = notificationRepository;
            _mapper = mapper;
            _rtNotificationService = rtNotificationService;
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
                    // Only notify if the reaction changes from anything else to Like.
                    bool shouldNotify =
                        existingReaction.Type != ReactionType.Like &&
                        type == ReactionType.Like;

                    existingReaction.Type = type;
                    await _reactionRepository.UpdateAsync(existingReaction.Id, existingReaction);

                    if (shouldNotify)
                    {
                        try
                        {
                            var post = await _postRepository.GetByIdAsync(postId);

                            if (post != null &&
                                !string.IsNullOrWhiteSpace(post.UserId) &&
                                post.UserId != userId)
                            {
                                var notificationDto = new NotificationDto
                                {
                                    UserId = post.UserId,
                                    ActorUserId = userId,
                                    Type = NotificationType.Like,
                                    IsRead = false,
                                    CreatedAt = DateTime.UtcNow,
                                    PostId = postId
                                };

                                var notification = _mapper.Map<Notification>(notificationDto);

                                await _notificationRepository.AddAsync(notification);

                                var unreadCount =
                                    await _notificationRepository.GetUnreadCountAsync(post.UserId);

                                await _rtNotificationService.SendNotificationAsync(
                                    userId: post.UserId,
                                    message: "Someone liked your post.",
                                    iconClass: "ph-fill ph-heart",
                                    unreadCount: unreadCount);
                            }
                        }
                        catch (Exception notifEx)
                        {
                            Console.WriteLine($"[Notif ERROR] {notifEx.GetType().Name}: {notifEx.Message}");

                            if (notifEx.InnerException != null)
                            {
                                Console.WriteLine($"[Notif INNER] {notifEx.InnerException.GetType().Name}: {notifEx.InnerException.Message}");
                            }

                            Console.WriteLine(notifEx.StackTrace);
                        }
                    }

                    return true;
                }

                // Create new reaction
                var newReaction = new PostReaction
                {
                    UserId = userId,
                    PostId = postId,
                    Type = type,
                    CreatedAt = DateTime.UtcNow
                };

                await _reactionRepository.AddAsync(newReaction);

                // Notify only when a NEW Like is created
                if (type == ReactionType.Like)
                {
                    try
                    {
                        var post = await _postRepository.GetByIdAsync(postId);

                        if (post != null &&
                            !string.IsNullOrWhiteSpace(post.UserId) &&
                            post.UserId != userId)
                        {
                            var notificationDto = new NotificationDto
                            {
                                UserId = post.UserId,
                                ActorUserId = userId,
                                Type = NotificationType.Like,
                                IsRead = false,
                                CreatedAt = DateTime.UtcNow,
                                PostId = postId
                            };

                            var notification = _mapper.Map<Notification>(notificationDto);

                            await _notificationRepository.AddAsync(notification);

                            var unreadCount =
                                await _notificationRepository.GetUnreadCountAsync(post.UserId);

                            await _rtNotificationService.SendNotificationAsync(
                                userId: post.UserId,
                                message: "Someone liked your post.",
                                iconClass: "ph-fill ph-heart",
                                unreadCount: unreadCount);
                        }
                    }
                    catch (Exception notifEx)
                    {
                        Console.WriteLine($"[Notif ERROR] {notifEx.GetType().Name}: {notifEx.Message}");

                        if (notifEx.InnerException != null)
                        {
                            Console.WriteLine($"[Notif INNER] {notifEx.InnerException.GetType().Name}: {notifEx.InnerException.Message}");
                        }

                        Console.WriteLine(notifEx.StackTrace);
                    }
                }

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

                var reactions = await query
                    .Where(r => r.PostId == postId)
                    .ToListAsync();

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
