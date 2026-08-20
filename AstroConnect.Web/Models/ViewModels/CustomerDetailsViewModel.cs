using AstroConnect.Domain.Entities;

namespace AstroConnect.Web.Models.ViewModels;

public class CustomerDetailsViewModel
{
    public Customer Customer { get; set; } = null!;

    public List<Booking> Bookings { get; set; } = new();
}