using AstroConnect.Domain.Common;
using AstroConnect.Domain.Enums;
using AstroConnect.Persistence.Context;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AstroConnect.Web.Controllers;

[Authorize(Roles = Roles.Admin)]
public class AdminController : Controller
{
    private readonly ApplicationDbContext _context;

    public AdminController(ApplicationDbContext context)
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
        // BASIC STATISTICS
        // -----------------------------------------------------

        var totalCustomers = await _context.Customers
            .CountAsync(c => !c.IsDeleted);

        var totalAstrologers = await _context.Astrologers
            .CountAsync(a => !a.IsDeleted);

        var totalServices = await _context.Services
            .CountAsync(s => !s.IsDeleted);

        var totalBookings = await _context.Bookings
            .CountAsync(b => !b.IsDeleted);


        // -----------------------------------------------------
        // BOOKING STATUS COUNTS
        // -----------------------------------------------------

        var pendingBookings = await _context.Bookings
            .CountAsync(b =>
                !b.IsDeleted &&
                b.Status == BookingStatus.Pending);

        var confirmedBookings = await _context.Bookings
            .CountAsync(b =>
                !b.IsDeleted &&
                b.Status == BookingStatus.Confirmed);

        var completedBookings = await _context.Bookings
            .CountAsync(b =>
                !b.IsDeleted &&
                b.Status == BookingStatus.Completed);

        var cancelledBookings = await _context.Bookings
            .CountAsync(b =>
                !b.IsDeleted &&
                b.Status == BookingStatus.Cancelled);


        // -----------------------------------------------------
        // TODAY
        // -----------------------------------------------------

        var today = DateOnly.FromDateTime(DateTime.Today);

        var todayBookings = await _context.Bookings
            .Include(b => b.Customer)
            .Include(b => b.Astrologer)
            .Include(b => b.Service)
            .Where(b =>
                !b.IsDeleted &&
                b.BookingDate == today)
            .OrderBy(b => b.StartTime)
            .ToListAsync();


        // -----------------------------------------------------
        // UPCOMING APPOINTMENTS
        // -----------------------------------------------------

        var upcomingBookings = await _context.Bookings
            .Include(b => b.Customer)
            .Include(b => b.Astrologer)
            .Include(b => b.Service)
            .Where(b =>
                !b.IsDeleted &&
                b.BookingDate >= today &&
                b.Status != BookingStatus.Cancelled &&
                b.Status != BookingStatus.Completed)
            .OrderBy(b => b.BookingDate)
            .ThenBy(b => b.StartTime)
            .Take(5)
            .ToListAsync();


        // -----------------------------------------------------
        // RECENT BOOKINGS
        // -----------------------------------------------------

        var recentBookings = await _context.Bookings
            .Include(b => b.Customer)
            .Include(b => b.Astrologer)
            .Include(b => b.Service)
            .Where(b => !b.IsDeleted)
            .OrderByDescending(b => b.Id)
            .Take(5)
            .ToListAsync();


        // -----------------------------------------------------
        // VIEWBAG - BASIC STATISTICS
        // -----------------------------------------------------

        ViewBag.TotalCustomers = totalCustomers;
        ViewBag.TotalAstrologers = totalAstrologers;
        ViewBag.TotalServices = totalServices;
        ViewBag.TotalBookings = totalBookings;


        // -----------------------------------------------------
        // VIEWBAG - BOOKING STATUS
        // -----------------------------------------------------

        ViewBag.PendingBookings = pendingBookings;
        ViewBag.ConfirmedBookings = confirmedBookings;
        ViewBag.CompletedBookings = completedBookings;
        ViewBag.CancelledBookings = cancelledBookings;


        // -----------------------------------------------------
        // VIEWBAG - TODAY
        // -----------------------------------------------------

        ViewBag.TodayBookings = todayBookings.Count;
        ViewBag.TodayAppointmentList = todayBookings;


        // -----------------------------------------------------
        // VIEWBAG - UPCOMING
        // -----------------------------------------------------

        ViewBag.UpcomingBookings = upcomingBookings;


        // -----------------------------------------------------
        // VIEWBAG - RECENT BOOKINGS
        // -----------------------------------------------------

        ViewBag.RecentBookings = recentBookings;


        return View();
    }
}