using AstroConnect.Domain.Common;
using AstroConnect.Domain.Enums;
using AstroConnect.Persistence.Context;
using AstroConnect.Web.ViewModels.Dashboard;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AstroConnect.Web.Controllers;

[Authorize(Roles = Roles.Admin)]
public class DashboardController : Controller
{
    private readonly ApplicationDbContext _context;

    public DashboardController(ApplicationDbContext context)
    {
        _context = context;
    }

    // =========================================================
    // ADMIN DASHBOARD
    // =========================================================

    [HttpGet]
    public async Task<IActionResult> Index()
    {
        // -----------------------------------------------------
        // BASIC DASHBOARD STATISTICS
        // -----------------------------------------------------

        var model = new AdminDashboardViewModel
        {
            TotalCustomers = await _context.Customers
                .CountAsync(c => !c.IsDeleted),

            TotalAstrologers = await _context.Astrologers
                .CountAsync(a => !a.IsDeleted),

            TotalServices = await _context.Services
                .CountAsync(s => !s.IsDeleted && s.IsActive),

            TotalBookings = await _context.Bookings
                .CountAsync(b => !b.IsDeleted),

            PendingBookings = await _context.Bookings
                .CountAsync(b =>
                    !b.IsDeleted &&
                    b.Status == BookingStatus.Pending),

            ConfirmedBookings = await _context.Bookings
                .CountAsync(b =>
                    !b.IsDeleted &&
                    b.Status == BookingStatus.Confirmed),

            CompletedBookings = await _context.Bookings
                .CountAsync(b =>
                    !b.IsDeleted &&
                    b.Status == BookingStatus.Completed),

            CancelledBookings = await _context.Bookings
                .CountAsync(b =>
                    !b.IsDeleted &&
                    b.Status == BookingStatus.Cancelled)
        };


        // -----------------------------------------------------
        // RECENT BOOKINGS
        // -----------------------------------------------------

        model.RecentBookings = await _context.Bookings
            .AsNoTracking()
            .Where(b => !b.IsDeleted)
            .OrderByDescending(b => b.Id)
            .Take(5)
            .Select(b => new RecentBookingViewModel
            {
                Id = b.Id,

                CustomerName = b.Customer != null
                    ? b.Customer.FullName
                    : "N/A",

                AstrologerName = b.Astrologer != null
                    ? b.Astrologer.FullName
                    : "N/A",

                ServiceName = b.Service != null
                    ? b.Service.Name
                    : "N/A",

                BookingDate = b.BookingDate,

                StartTime = b.StartTime,

                EndTime = b.EndTime,

                Status = b.Status.ToString()
            })
            .ToListAsync();


        // -----------------------------------------------------
        // TODAY'S APPOINTMENTS
        // -----------------------------------------------------

        var today = DateOnly.FromDateTime(DateTime.Today);

        model.TodayAppointments = await _context.Bookings
            .AsNoTracking()
            .Where(b =>
                !b.IsDeleted &&
                b.BookingDate == today &&
                b.Status != BookingStatus.Cancelled)
            .OrderBy(b => b.StartTime)
            .Select(b => new RecentBookingViewModel
            {
                Id = b.Id,

                CustomerName = b.Customer != null
                    ? b.Customer.FullName
                    : "N/A",

                AstrologerName = b.Astrologer != null
                    ? b.Astrologer.FullName
                    : "N/A",

                ServiceName = b.Service != null
                    ? b.Service.Name
                    : "N/A",

                BookingDate = b.BookingDate,

                StartTime = b.StartTime,

                EndTime = b.EndTime,

                Status = b.Status.ToString()
            })
            .ToListAsync();


        return View(model);
    }
}