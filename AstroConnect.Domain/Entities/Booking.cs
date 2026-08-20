using AstroConnect.Domain.Common;
using AstroConnect.Domain.Enums;

namespace AstroConnect.Domain.Entities;

public class Booking : BaseEntity
{
    // Foreign Keys
    public int ServiceId { get; set; }

    public int AstrologerId { get; set; }
    public int CustomerId { get; set; }

    // Booking Details
    public DateOnly BookingDate { get; set; }

    public TimeOnly StartTime { get; set; }

    public TimeOnly EndTime { get; set; }

    public BookingStatus Status { get; set; } = BookingStatus.Pending;

    // Customer Details
    public string WhatsAppNumber { get; set; } = string.Empty;

    public string Gender { get; set; } = string.Empty;

    // Birth Details
    public DateOnly BirthDate { get; set; }

    public TimeOnly BirthTime { get; set; }

    public string BirthPlace { get; set; } = string.Empty;

    // Optional Question
    public string? Question { get; set; }

    // Navigation Properties
    public Customer Customer { get; set; } = null!;

    public Service Service { get; set; } = null!;

    public Astrologer Astrologer { get; set; } = null!;
    
}