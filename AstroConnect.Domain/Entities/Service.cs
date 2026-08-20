using AstroConnect.Domain.Common;

namespace AstroConnect.Domain.Entities;

public class Service : BaseEntity
{
    public string Name { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public decimal Price { get; set; }

    public int DurationInMinutes { get; set; }

    public bool IsActive { get; set; } = true;
}