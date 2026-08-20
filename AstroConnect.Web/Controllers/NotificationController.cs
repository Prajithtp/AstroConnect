using AstroConnect.Application.Interfaces.Repositories;
using AstroConnect.Domain.Common;
using AstroConnect.Persistence.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace AstroConnect.Web.Controllers;

[Authorize(Roles = Roles.Customer)]
public class NotificationController : Controller
{
    private readonly INotificationRepository _notificationRepository;
    private readonly UserManager<ApplicationUser> _userManager;

    public NotificationController(
        INotificationRepository notificationRepository,
        UserManager<ApplicationUser> userManager)
    {
        _notificationRepository = notificationRepository;
        _userManager = userManager;
    }


    // =========================================================
    // ALL NOTIFICATIONS
    // =========================================================

    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var user = await _userManager.GetUserAsync(User);

        if (user == null)
            return RedirectToAction("Login", "Account");

        var notifications =
            await _notificationRepository.GetByUserIdAsync(user.Id);

        return View(notifications);
    }


    // =========================================================
    // UNREAD NOTIFICATION COUNT
    // =========================================================

    [HttpGet]
    public async Task<IActionResult> UnreadCount()
    {
        var user = await _userManager.GetUserAsync(User);

        if (user == null)
            return Json(new { count = 0 });

        var count =
            await _notificationRepository.GetUnreadCountAsync(user.Id);

        return Json(new { count });
    }


    // =========================================================
    // MARK ONE AS READ
    // =========================================================

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> MarkAsRead(int id)
    {
        var user = await _userManager.GetUserAsync(User);

        if (user == null)
            return RedirectToAction("Login", "Account");

        var notification =
            await _notificationRepository.GetByIdAsync(id);

        if (notification == null)
            return NotFound();

        // Customer can only modify their own notification.
        if (notification.UserId != user.Id)
            return Forbid();

        await _notificationRepository.MarkAsReadAsync(id);

        return RedirectToAction(nameof(Index));
    }


    // =========================================================
    // MARK ALL AS READ
    // =========================================================

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> MarkAllAsRead()
    {
        var user = await _userManager.GetUserAsync(User);

        if (user == null)
            return RedirectToAction("Login", "Account");

        await _notificationRepository
            .MarkAllAsReadAsync(user.Id);

        TempData["Success"] =
            "All notifications marked as read.";

        return RedirectToAction(nameof(Index));
    }
}