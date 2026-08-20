using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace AstroConnect.Web.ViewModels.Booking;

public class BookingCreateViewModel
{
    [Required]
    public int CustomerId { get; set; }

    [Required]
    public int AstrologerId { get; set; }

    [Required]
    public int ServiceId { get; set; }

    [Required]
    public DateOnly BookingDate { get; set; }

    [Required]
    public TimeOnly StartTime { get; set; }

    [Required]
    public TimeOnly EndTime { get; set; }

    [Required]
    public string WhatsAppNumber { get; set; } = string.Empty;

    [Required]
    public string Gender { get; set; } = string.Empty;

    [Required]
    public DateOnly BirthDate { get; set; }

    [Required]
    public TimeOnly BirthTime { get; set; }

    [Required]
    public string BirthPlace { get; set; } = string.Empty;

    public string? Question { get; set; }

    public List<SelectListItem> Customers { get; set; } = new();
    public List<SelectListItem> Astrologers { get; set; } = new();
    public List<SelectListItem> Services { get; set; } = new();
}