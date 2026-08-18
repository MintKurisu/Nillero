using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Nillero.Core.Application.Interfaces.Notifications;
using Nillero.Core.Application.Interfaces.Presentation.Mappers;
using Nillero.Core.Domain.Common.Enum;
using Nillero.Infrastructure.Identity.Entities;
using NilleroWebApp.Controllers;
using NilleroWebApp.Hubs;

namespace Nillero.Controllers
{
    [Authorize]
    [Route("notifications")]
    public class NotificationController : BaseController
    {
        private readonly INotificationService _notificationService;
        private readonly IHubContext<NotificationHub> _hubContext;
        private readonly INotificationViewModelMapper _notificationViewModelMapper;

        public NotificationController(
            UserManager<ApplicationUser> userManager,
            INotificationService notificationService,
            IHubContext<NotificationHub> hubContext,
            INotificationViewModelMapper notificationViewModelMapper)
            : base(userManager)
        {
            _notificationService = notificationService;
            _hubContext = hubContext;
            _notificationViewModelMapper = notificationViewModelMapper;
        }

        private string CurrentUserId =>
            User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value
            ?? throw new InvalidOperationException("Authenticated user has no identifier claim.");

        [HttpGet("")]
        public async Task<IActionResult> Index()
        {
            ViewData["ActiveNav"] = "notifications";
            var notifications =
                await _notificationService.GetForUserAsync(CurrentUserId);
            var viewModels =
                await _notificationViewModelMapper.MapAsync(notifications);
            return View(viewModels);
        }

        [HttpGet("{id:int}/navigate")]
        public async Task<IActionResult> Navigate(int id)
        {
            var userId = CurrentUserId;

            var notification = await _notificationService.GetById(id);

            if (notification == null || notification.UserId != userId)
                return RedirectToAction(nameof(Index));

            await _notificationService.MarkAsReadAsync(id, userId);

            var unreadCount = await _notificationService.GetUnreadCountAsync(userId);
            await _hubContext.Clients.User(userId)
                .SendAsync("UpdateUnreadCount", unreadCount);

            return notification.Type switch
            {
                NotificationType.Comment => RedirectToAction("Details", "Home", new { id = notification.PostId }),
                NotificationType.Reply => RedirectToAction("Details", "Home", new { id = notification.PostId }),
                NotificationType.Like => RedirectToAction("Details", "Home", new { id = notification.PostId }),
                NotificationType.FriendRequest => RedirectToAction("Index", "FriendRequest"),
                NotificationType.FriendAccepted => RedirectToAction("UserPosts", "Friends", new { userId = notification.ActorUserId }),
                _ => RedirectToAction(nameof(Index))
            };
        }

        [HttpPost("{id:int}/read")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> MarkAsRead(int id)
        {
            var userId = CurrentUserId;
            var success = await _notificationService.MarkAsReadAsync(id, userId);
            if (success)
            {
                var unreadCount = await _notificationService.GetUnreadCountAsync(userId);
                await _hubContext.Clients.User(userId)
                    .SendAsync("UpdateUnreadCount", unreadCount);
            }

            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            {
                return Json(new { success });
            }

            return RedirectToAction(nameof(Index));
        }

        [HttpPost("read-all")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> MarkAllAsRead()
        {
            var userId = CurrentUserId;
            await _notificationService.MarkAllAsReadAsync(userId);
            await _hubContext.Clients.User(userId)
                .SendAsync("UpdateUnreadCount", 0);

            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            {
                return Json(new { success = true });
            }

            return RedirectToAction(nameof(Index));
        }

        [HttpGet("unread-count")]
        public async Task<IActionResult> UnreadCount()
        {
            var count = await _notificationService.GetUnreadCountAsync(CurrentUserId);
            return Json(new { count });
        }

        [HttpGet("dropdown")]
        public async Task<IActionResult> Dropdown()
        {
            var notifications = await _notificationService.GetForUserAsync(CurrentUserId);
            var viewModels = await _notificationViewModelMapper.MapAsync(notifications);
            var latest = viewModels.Take(5).ToList();
            return PartialView("_NotificationDropdown", latest);
        }

        [HttpPost("{id:int}/delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var userId = CurrentUserId;
            var notification = await _notificationService.GetById(id);

            if (notification == null || notification.UserId != userId)
                return Request.Headers["X-Requested-With"] == "XMLHttpRequest"
                    ? Json(new { success = false })
                    : RedirectToAction(nameof(Index));

            var success = await _notificationService.DeleteAsync(id);

            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                return Json(new { success });

            return RedirectToAction(nameof(Index));
        }

        [HttpPost("delete-all")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteAll()
        {
            var userId = CurrentUserId;
            var notifications = await _notificationService.GetForUserAsync(userId);

            foreach (var n in notifications)
                await _notificationService.DeleteAsync(n.Id);

            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                return Json(new { success = true });

            return RedirectToAction(nameof(Index));
        }
    }
}