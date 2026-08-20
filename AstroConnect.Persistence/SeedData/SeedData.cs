using AstroConnect.Domain.Entities;
using AstroConnect.Persistence.Context;
using Microsoft.EntityFrameworkCore;

namespace AstroConnect.Persistence.SeedData;

public static class SeedData
{
    public static async Task InitializeAsync(ApplicationDbContext context)
    {
        if (await context.Services.AnyAsync())
            return;

        var service = new Service
        {
            Name = "Marriage Consultation",
            Description = "Personalized guidance for marriage and relationships.",
            Price = 500,
            DurationInMinutes = 30,
            IsActive = true
        };

        await context.Services.AddAsync(service);

        await context.SaveChangesAsync();
    }
}