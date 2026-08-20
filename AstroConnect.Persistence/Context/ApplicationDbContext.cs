using AstroConnect.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using AstroConnect.Persistence.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;

namespace AstroConnect.Persistence.Context;

public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }
    public DbSet<Service> Services { get; set; }

    public DbSet<Astrologer> Astrologers { get; set; }
    public DbSet<Customer> Customers { get; set; }
    public DbSet<Booking> Bookings { get; set; }
    public DbSet<Notification> Notifications { get; set; }

}