using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Nillero.Core.Application.Dtos.Notifications;
using Nillero.Core.Application.Dtos.Social;
using Nillero.Core.Application.Interfaces.Notifications;
using Nillero.Core.Application.Interfaces.Social;
using Nillero.Core.Application.Services.Base;
using Nillero.Core.Domain.Common.Enum;
using Nillero.Core.Domain.Entities.Notifications;
using Nillero.Core.Domain.Entities.Social;
using Nillero.Core.Domain.Interfaces.Notifications;
using Nillero.Core.Domain.Interfaces.Social;

namespace Nillero.Core.Application.Services.Social
{
    public class CommentService : GenericService<Comment, CommentDto>, ICommentService
    {
        private readonly ICommentRepository _commentRepository;
        private readonly IPostRepository _postRepository;
        private readonly INotificationRepository _notificationRepository;
        private readonly IMapper _mapper;
        private readonly IRealTimeNotificationService _rtNotificationService;

        public CommentService(
            ICommentRepository commentRepository,
            IPostRepository postRepository,
            INotificationRepository notificationRepository,
            IMapper mapper,
            IRealTimeNotificationService rtNotificationService) : base(commentRepository, mapper)
        {
            _commentRepository = commentRepository;
            _postRepository = postRepository;
            _notificationRepository = notificationRepository;
            _mapper = mapper;
            _rtNotificationService = rtNotificationService;
        }

        public override async Task<CommentDto?> AddAsync(CommentDto dto)
        {
            try
            {
                if (dto.ParentCommentId.HasValue)
                {
                    var parent = await _commentRepository.GetByIdAsync(dto.ParentCommentId.Value);
                    if (parent != null)
                    {
                        dto.RootCommentId = parent.RootCommentId ?? parent.Id;
                    }
                }

                var entity = _mapper.Map<Comment>(dto);
                var result = await _commentRepository.AddAsync(entity);
                var resultDto = _mapper.Map<CommentDto>(result);

                try
                {
                    string? targetUserId = null;

                    if (dto.ParentCommentId.HasValue)
                    {
                        var parentComment = await _commentRepository.GetByIdAsync(dto.ParentCommentId.Value);
                        targetUserId = parentComment?.UserId;
                    }
                    else
                    {
                        var post = await _postRepository.GetByIdAsync(dto.PostId);
                        targetUserId = post?.UserId;
                    }

                    if (!string.IsNullOrEmpty(targetUserId) && targetUserId != dto.UserId)
                    {
                        // Temporary message used only for the real-time notification.

                        string textMessage = dto.ParentCommentId.HasValue
                            ? "Someone replied to your comment."
                            : "Someone commented on your post.";

                        // Create a clean DTO for the notification

                        var notificationDto = new NotificationDto
                        {
                            UserId = targetUserId,
                            ActorUserId = dto.UserId,
                            Type = dto.ParentCommentId.HasValue
                                ? NotificationType.Reply
                                : NotificationType.Comment,
                            IsRead = false,
                            CreatedAt = DateTime.UtcNow,

                            PostId = dto.PostId,
                            CommentId = resultDto.Id
                        };

                        var dbNotification = _mapper.Map<Notification>(notificationDto);
                        await _notificationRepository.AddAsync(dbNotification);

                        // Obtain the total unread notifications count for the user
                        int totalUnread = await _notificationRepository.GetUnreadCountAsync(targetUserId);

                        // Push in real-time
                        await _rtNotificationService.SendNotificationAsync(
                            userId: targetUserId,
                            message: textMessage,
                            iconClass: "ph-fill ph-chat-circle",
                            unreadCount: totalUnread
                        );
                    }
                }
                catch (Exception notifEx)
                {
                    /*Console.WriteLine($"Non-fatal error running real-time notification subsystem: {notifEx.Message}");*/
                    Console.WriteLine($"[Notif ERROR] {notifEx.GetType().Name}: {notifEx.Message}");
                    if (notifEx.InnerException != null)
                        Console.WriteLine($"[Notif INNER] {notifEx.InnerException.GetType().Name}: {notifEx.InnerException.Message}");
                    Console.WriteLine($"Stack: {notifEx.StackTrace}");

                }

                return resultDto;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in AddAsync (Comment): {ex.Message}");
                return null;
            }
        }

        public async Task<List<CommentDto>> GetCommentsByPostIdAsync(int postId)
        {
            try
            {
                // Bring everything flat, without including Replies
                var allComments = await _commentRepository
                    .GetAllQueryWithInclude(new List<string>())
                    .Where(c => c.PostId == postId)
                    .OrderBy(c => c.CreatedAt)
                    .ToListAsync();

                // Separate roots and replies
                var roots = allComments
                    .Where(c => c.ParentCommentId == null)
                    .ToList();

                var repliesByRoot = allComments
                    .Where(c => c.RootCommentId.HasValue)
                    .GroupBy(c => c.RootCommentId!.Value)
                    .ToDictionary(g => g.Key, g => g.OrderBy(r => r.CreatedAt).ToList());

                // Build a tree with a single level of replies
                var result = new List<CommentDto>();

                foreach (var root in roots)
                {
                    var rootDto = _mapper.Map<CommentDto>(root);
                    rootDto.Replies = repliesByRoot.TryGetValue(root.Id, out var replies)
                        ? _mapper.Map<List<CommentDto>>(replies)
                        : new List<CommentDto>();

                    //  Replies do not have children
                    foreach (var reply in rootDto.Replies)
                        reply.Replies = new List<CommentDto>();

                    result.Add(rootDto);
                }

                return result;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in GetCommentsByPostIdAsync: {ex.Message}");
                throw;
            }
        }
    }
}
