
using AstroConnect.Domain.Entities;

namespace AstroConnect.Web.ViewModels.Receptionist;

public class ReceptionistDashboardViewModel
{
    // Booking statistics
    public int TodayBookings { get; set; }

    public int PendingBookings { get; set; }

    public int ConfirmedBookings { get; set; }

    public int CompletedBookings { get; set; }

    public int CancelledBookings { get; set; }


    // Today's appointments
    public List<AstroConnect.Domain.Entities.Booking> TodayAppointmentList { get; set; } = new();


    // Upcoming appointments
    public List<AstroConnect.Domain.Entities.Booking> UpcomingAppointmentList { get; set; } = new();


    // Recent bookings
    public List<AstroConnect.Domain.Entities.Booking> RecentBookingList { get; set; } = new();
}
