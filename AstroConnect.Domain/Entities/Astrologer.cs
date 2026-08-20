using AstroConnect.Domain.Common;

namespace AstroConnect.Domain.Entities;

public class Astrologer : BaseEntity
{
    public string FullName { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;

    public string PhoneNumber { get; set; } = string.Empty;

    public int Experience { get; set; }

    public string Specialization { get; set; } = string.Empty;

    public string Language { get; set; } = "Malayalam";

    public string Biography { get; set; } = string.Empty;

    public string ProfileImageUrl { get; set; } = string.Empty;

    public bool IsActive { get; set; } = true;
    public ICollection<Booking> Bookings { get; set; } = new List<Booking>();
}