using AstroConnect.Domain.Common;

namespace AstroConnect.Domain.Entities;

public class Customer : BaseEntity
{
    public string UserId { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;

    public string PhoneNumber { get; set; } = string.Empty;

    public DateTime DateOfBirth { get; set; }

    public string Gender { get; set; } = string.Empty;

    public string BirthTime { get; set; } = string.Empty;

    public string BirthPlace { get; set; } = string.Empty;

    public string Address { get; set; } = string.Empty;
}