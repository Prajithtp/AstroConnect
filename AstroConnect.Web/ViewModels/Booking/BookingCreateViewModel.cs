using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace AstroConnect.Web.ViewModels.Booking;

public class BookingCreateViewModel
{
    // =========================================================
    // BOOKING RELATIONSHIPS
    // =========================================================

    [Range(1, int.MaxValue, ErrorMessage = "Please select a customer.")]
    public int CustomerId { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "Please select an astrologer.")]
    public int AstrologerId { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "Please select a service.")]
    public int ServiceId { get; set; }


    // =========================================================
    // APPOINTMENT
    // =========================================================

    [Required(ErrorMessage = "Booking date is required.")]
    public DateOnly BookingDate { get; set; }

    [Required(ErrorMessage = "Start time is required.")]
    public TimeOnly StartTime { get; set; }

    [Required(ErrorMessage = "End time is required.")]
    public TimeOnly EndTime { get; set; }


    // =========================================================
    // CUSTOMER DETAILS
    // =========================================================

    [Required(ErrorMessage = "WhatsApp number is required.")]
    [StringLength(
        20,
        MinimumLength = 7,
        ErrorMessage = "WhatsApp number must be between 7 and 20 characters.")]
    public string WhatsAppNumber { get; set; } = string.Empty;

    [Required(ErrorMessage = "Gender is required.")]
    [StringLength(
        30,
        ErrorMessage = "Gender cannot exceed 30 characters.")]
    public string Gender { get; set; } = string.Empty;

    [Required(ErrorMessage = "Birth date is required.")]
    public DateOnly BirthDate { get; set; }

    [Required(ErrorMessage = "Birth time is required.")]
    public TimeOnly BirthTime { get; set; }

    [Required(ErrorMessage = "Birth place is required.")]
    [StringLength(
        150,
        ErrorMessage = "Birth place cannot exceed 150 characters.")]
    public string BirthPlace { get; set; } = string.Empty;

    [StringLength(
        1000,
        ErrorMessage = "Question cannot exceed 1000 characters.")]
    public string? Question { get; set; }


    // =========================================================
    // DROPDOWNS
    // =========================================================

    public List<SelectListItem> Customers { get; set; } = new();

    public List<SelectListItem> Astrologers { get; set; } = new();

    public List<SelectListItem> Services { get; set; } = new();
}