using AstroConnect.Domain.Entities;
using AstroConnect.Persistence.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace AstroConnect.Persistence.Context;

public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
{
    public ApplicationDbContext(
        DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }


    // =========================================================
    // DB SETS
    // =========================================================

    public DbSet<Service> Services { get; set; }

    public DbSet<Astrologer> Astrologers { get; set; }

    public DbSet<Customer> Customers { get; set; }

    public DbSet<Booking> Bookings { get; set; }

    public DbSet<Notification> Notifications { get; set; }


    // =========================================================
    // MODEL CONFIGURATION
    // =========================================================

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Service price is stored as decimal(18,2).
        // This prevents EF Core from relying on SQL Server's
        // implicit/default decimal precision.
        modelBuilder.Entity<Service>()
            .Property(s => s.Price)
            .HasPrecision(18, 2);
    }
}