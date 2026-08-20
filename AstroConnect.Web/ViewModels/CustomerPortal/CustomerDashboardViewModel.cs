using AstroConnect.Domain.Entities;

namespace AstroConnect.Web.ViewModels.CustomerPortal;

public class CustomerDashboardViewModel
{
    // =========================================================
    // BOOKING STATISTICS
    // =========================================================

    public int TotalBookings { get; set; }

    public int PendingBookings { get; set; }

    public int ConfirmedBookings { get; set; }

    public int CompletedBookings { get; set; }


    // =========================================================
    // UPCOMING APPOINTMENT
    // =========================================================

    public AstroConnect.Domain.Entities.Booking? UpcomingBooking { get; set; }


    // =========================================================
    // RECENT BOOKINGS
    // =========================================================

    public List<AstroConnect.Domain.Entities.Booking> RecentBookings { get; set; } = new();


    // =========================================================
    // NOTIFICATIONS
    // =========================================================

    public int UnreadNotificationCount { get; set; }
}