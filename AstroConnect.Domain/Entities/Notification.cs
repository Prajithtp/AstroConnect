namespace AstroConnect.Domain.Entities;

public class Notification
{
    public int Id { get; set; }

    // The customer/user who receives the notification
    public string UserId { get; set; } = string.Empty;

    // Notification title
    public string Title { get; set; } = string.Empty;

    // Notification message
    public string Message { get; set; } = string.Empty;

    // Optional booking related to this notification
    public int? BookingId { get; set; }

    // Has the customer read the notification?
    public bool IsRead { get; set; }

    // When the notification was created
    public DateTime CreatedAt { get; set; } = DateTime.Now;

    // Navigation property
    public Booking? Booking { get; set; }
}