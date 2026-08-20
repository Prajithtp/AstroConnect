namespace AstroConnect.Web.ViewModels.Dashboard;

public class AdminDashboardViewModel
{
    // =========================================================
    // DASHBOARD STATISTICS
    // =========================================================

    public int TotalCustomers { get; set; }

    public int TotalAstrologers { get; set; }

    public int TotalServices { get; set; }

    public int TotalBookings { get; set; }

    public int PendingBookings { get; set; }

    public int ConfirmedBookings { get; set; }

    public int CompletedBookings { get; set; }

    public int CancelledBookings { get; set; }


    // =========================================================
    // RECENT BOOKINGS
    // =========================================================

    public List<RecentBookingViewModel> RecentBookings { get; set; }
        = new List<RecentBookingViewModel>();


    // =========================================================
    // TODAY'S APPOINTMENTS
    // =========================================================

    public List<RecentBookingViewModel> TodayAppointments { get; set; }
        = new List<RecentBookingViewModel>();
}


// =============================================================
// RECENT BOOKING MODEL
// =============================================================

public class RecentBookingViewModel
{
    public int Id { get; set; }

    public string CustomerName { get; set; }
        = string.Empty;

    public string AstrologerName { get; set; }
        = string.Empty;

    public string ServiceName { get; set; }
        = string.Empty;

    public DateOnly BookingDate { get; set; }

    public TimeOnly StartTime { get; set; }

    public TimeOnly EndTime { get; set; }

    public string Status { get; set; }
        = string.Empty;
}