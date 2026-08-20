namespace AstroConnect.Web.ViewModels.Receptionist;

public class ReceptionistBookingCalendarViewModel
{
    public DateOnly SelectedDate { get; set; }

    public List<BookingCalendarItemViewModel> Bookings { get; set; } = new();
}

public class BookingCalendarItemViewModel
{
    public int Id { get; set; }

    public string CustomerName { get; set; } = string.Empty;

    public string AstrologerName { get; set; } = string.Empty;

    public string ServiceName { get; set; } = string.Empty;

    public DateOnly BookingDate { get; set; }

    public TimeOnly StartTime { get; set; }

    public TimeOnly EndTime { get; set; }

    public string Status { get; set; } = string.Empty;
}