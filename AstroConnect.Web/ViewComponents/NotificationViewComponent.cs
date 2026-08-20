using AstroConnect.Application.Interfaces.Repositories;
using AstroConnect.Persistence.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace AstroConnect.Web.ViewComponents;

public class NotificationViewComponent : ViewComponent
{
    private readonly INotificationRepository _notificationRepository;
    private readonly UserManager<ApplicationUser> _userManager;

    public NotificationViewComponent(
        INotificationRepository notificationRepository,
        UserManager<ApplicationUser> userManager)
    {
        _notificationRepository = notificationRepository;
        _userManager = userManager;
    }

    public async Task<IViewComponentResult> InvokeAsync()
    {
        if (!User.Identity?.IsAuthenticated ?? true)
        {
            return View(0);
        }

        var user = await _userManager.GetUserAsync(UserClaimsPrincipal);

        if (user == null)
        {
            return View(0);
        }

        var unreadCount =
            await _notificationRepository
                .GetUnreadCountAsync(user.Id);

        return View(unreadCount);
    }
}