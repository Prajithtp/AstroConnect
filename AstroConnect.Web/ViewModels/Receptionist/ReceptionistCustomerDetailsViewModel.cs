using AstroConnect.Domain.Entities;

namespace AstroConnect.Web.ViewModels.Receptionist;

public class ReceptionistCustomerDetailsViewModel
{
    public Customer Customer { get; set; } = null!;

    public List<AstroConnect.Domain.Entities.Booking> Bookings { get; set; } = new();
}