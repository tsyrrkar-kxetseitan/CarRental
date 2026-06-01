using CarRental.Web.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace CarRental.Web.Controllers
{
    /// <summary>
    /// Handles in-site notification operations: list, mark read, mark all read.
    /// </summary>
    [Authorize]
    public class NotificationController : Controller
    {
        private readonly INotificationService _notificationService;

        public NotificationController(INotificationService notificationService)
        {
            _notificationService = notificationService;
        }

        // GET: /Notification/GetNotifications
        [HttpGet]
        public async Task<IActionResult> GetNotifications()
        {
            var userId = GetUserId();
            if (userId == null) return Json(new { notifications = Array.Empty<object>(), unreadCount = 0 });

            var notifications = await _notificationService.GetUserNotificationsAsync(userId.Value, 15);
            var unreadCount = await _notificationService.GetUnreadCountAsync(userId.Value);

            var result = notifications.Select(n => new
            {
                n.Id,
                n.Title,
                n.Message,
                n.Type,
                n.IsRead,
                CreatedAt = n.CreatedAt.ToString("dd MMM yyyy HH:mm"),
                TimeAgo = GetTimeAgo(n.CreatedAt)
            });

            return Json(new { notifications = result, unreadCount });
        }

        // POST: /Notification/MarkAsRead/5
        [HttpPost]
        public async Task<IActionResult> MarkAsRead(int id)
        {
            var userId = GetUserId();
            if (userId == null) return Unauthorized();

            await _notificationService.MarkAsReadAsync(id, userId.Value);
            return Ok();
        }

        // POST: /Notification/MarkAllAsRead
        [HttpPost]
        public async Task<IActionResult> MarkAllAsRead()
        {
            var userId = GetUserId();
            if (userId == null) return Unauthorized();

            await _notificationService.MarkAllAsReadAsync(userId.Value);
            return Ok();
        }

        private int? GetUserId()
        {
            var claim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return int.TryParse(claim, out int id) ? id : null;
        }

        private static string GetTimeAgo(DateTime dateTime)
        {
            var span = DateTime.Now - dateTime;
            if (span.TotalMinutes < 1) return "just now";
            if (span.TotalMinutes < 60) return $"{(int)span.TotalMinutes}m ago";
            if (span.TotalHours < 24) return $"{(int)span.TotalHours}h ago";
            if (span.TotalDays < 7) return $"{(int)span.TotalDays}d ago";
            return dateTime.ToString("dd MMM yyyy");
        }
    }
}
