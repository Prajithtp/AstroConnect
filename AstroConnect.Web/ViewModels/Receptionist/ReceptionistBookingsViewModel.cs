using AstroConnect.Domain.Entities;

namespace AstroConnect.Web.ViewModels.Receptionist;

public class ReceptionistBookingsViewModel
{
    public List<AstroConnect.Domain.Entities.Booking> Bookings { get; set; } = new();

    public string? SelectedStatus { get; set; }

    public DateOnly? SelectedDate { get; set; }
}