using AstroConnect.Application.Interfaces.Repositories;
using AstroConnect.Domain.Common;
using AstroConnect.Domain.Enums;
using AstroConnect.Domain.Entities;
using AstroConnect.Persistence.Context;
using AstroConnect.Web.ViewModels.Receptionist;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AstroConnect.Web.Controllers;

[Authorize(Roles = Roles.Receptionist)]
public class ReceptionistController : Controller
{
    private readonly IBookingRepository _bookingRepository;
    private readonly INotificationRepository _notificationRepository;
    private readonly ApplicationDbContext _context;

    public ReceptionistController(
        IBookingRepository bookingRepository,
        INotificationRepository notificationRepository,
        ApplicationDbContext context)
    {
        _bookingRepository = bookingRepository;
        _notificationRepository = notificationRepository;
        _context = context;
    }


    // =========================================================
    // RECEPTIONIST DASHBOARD
    // =========================================================

    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var bookings = await _bookingRepository.GetAllAsync();

        var activeBookings = bookings
            .Where(b => !b.IsDeleted)
            .ToList();

        var today = DateOnly.FromDateTime(DateTime.Today);

        var now = DateTime.Now;


        // =====================================================
        // TODAY'S APPOINTMENTS
        // =====================================================

        var todayAppointments = activeBookings
            .Where(b => b.BookingDate == today)
            .OrderBy(b => b.StartTime)
            .ToList();


        // =====================================================
        // UPCOMING APPOINTMENTS
        // =====================================================

        var upcomingAppointments = activeBookings
            .Where(b =>
                (b.Status == BookingStatus.Pending ||
                 b.Status == BookingStatus.Confirmed) &&
                b.BookingDate.ToDateTime(b.StartTime) > now)
            .OrderBy(b => b.BookingDate)
            .ThenBy(b => b.StartTime)
            .Take(5)
            .ToList();


        // =====================================================
        // RECENT BOOKINGS
        // =====================================================

        var recentBookings = activeBookings
            .OrderByDescending(b => b.BookingDate)
            .ThenByDescending(b => b.StartTime)
            .Take(5)
            .ToList();


        // =====================================================
        // DASHBOARD MODEL
        // =====================================================

        var model = new ReceptionistDashboardViewModel
        {
            // Statistics

            TodayBookings = todayAppointments.Count,

            PendingBookings = activeBookings.Count(b =>
                b.Status == BookingStatus.Pending),

            ConfirmedBookings = activeBookings.Count(b =>
                b.Status == BookingStatus.Confirmed),

            CompletedBookings = activeBookings.Count(b =>
                b.Status == BookingStatus.Completed),

            CancelledBookings = activeBookings.Count(b =>
                b.Status == BookingStatus.Cancelled),


            // Today's appointments

            TodayAppointmentList = todayAppointments,


            // Upcoming appointments

            UpcomingAppointmentList = upcomingAppointments,


            // Recent bookings

            RecentBookingList = recentBookings
        };

        return View(model);
    }


    // =========================================================
    // RECEPTIONIST BOOKING MANAGEMENT
    // =========================================================

    [HttpGet]
    public async Task<IActionResult> Bookings(
        string? status = null,
        DateOnly? date = null)
    {
        var bookings = await _bookingRepository.GetAllAsync();

        var filteredBookings = bookings
            .Where(b => !b.IsDeleted)
            .AsEnumerable();

        // Filter by status

        if (!string.IsNullOrWhiteSpace(status) &&
            Enum.TryParse<BookingStatus>(
                status,
                true,
                out var bookingStatus))
        {
            filteredBookings = filteredBookings
                .Where(b => b.Status == bookingStatus);
        }

        // Filter by date

        if (date.HasValue)
        {
            filteredBookings = filteredBookings
                .Where(b => b.BookingDate == date.Value);
        }

        var model = new ReceptionistBookingsViewModel
        {
            Bookings = filteredBookings
                .OrderByDescending(b => b.BookingDate)
                .ThenBy(b => b.StartTime)
                .ToList()
        };
        return View(model);
    }


    // =========================================================
    // BOOKING DETAILS
    // =========================================================

    [HttpGet]
    public async Task<IActionResult> BookingDetails(int id)
    {
        var booking = await _bookingRepository.GetByIdAsync(id);

        if (booking == null || booking.IsDeleted)
        {
            return NotFound();
        }

        return View(booking);
    }
    // =========================================================
    // BOOKING CALENDAR
    // =========================================================

    [HttpGet]
    public async Task<IActionResult> Calendar(DateOnly? date)
    {
        var selectedDate = date ?? DateOnly.FromDateTime(DateTime.Today);

        var bookings = await _bookingRepository.GetAllAsync();

        var calendarBookings = bookings
            .Where(b =>
                !b.IsDeleted &&
                b.BookingDate == selectedDate)
            .OrderBy(b => b.StartTime)
            .Select(b => new BookingCalendarItemViewModel
            {
                Id = b.Id,

                CustomerName =
                    b.Customer?.FullName ?? "Customer",

                AstrologerName =
                    b.Astrologer?.FullName ?? "Astrologer",

                ServiceName =
                    b.Service?.Name ?? "Consultation",

                BookingDate = b.BookingDate,

                StartTime = b.StartTime,

                EndTime = b.EndTime,

                Status = b.Status.ToString()
            })
            .ToList();

        var model = new ReceptionistBookingCalendarViewModel
        {
            SelectedDate = selectedDate,
            Bookings = calendarBookings
        };

        return View(model);
    }
    // =========================================================
    // RECEPTIONIST CUSTOMER DETAILS
    // =========================================================

    [HttpGet]
    public async Task<IActionResult> CustomerDetails(int id)
    {
        // Get customer
        var customer = await _context.Customers
            .FirstOrDefaultAsync(c =>
                c.Id == id &&
                !c.IsDeleted);

        if (customer == null)
        {
            return NotFound();
        }

        // Get customer's bookings
        var bookings = await _bookingRepository
            .GetByCustomerIdAsync(id);

        // Remove deleted bookings
        bookings = bookings
            .Where(b => !b.IsDeleted)
            .OrderByDescending(b => b.BookingDate)
            .ThenByDescending(b => b.StartTime)
            .ToList();

        // Create ViewModel
        var model = new ReceptionistCustomerDetailsViewModel
        {
            Customer = customer,
            Bookings = bookings
        };

        return View(model);
    }

    // =========================================================
    // CONFIRM BOOKING
    // =========================================================
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ConfirmBooking(int id)
    {
        var booking = await _bookingRepository.GetByIdAsync(id);

        if (booking == null)
            return NotFound();

        // Only pending bookings can be confirmed

        if (booking.Status != BookingStatus.Pending)
        {
            TempData["Error"] =
                "Only pending bookings can be confirmed.";

            return RedirectToAction(nameof(Index));
        }


        // -----------------------------------------------------
        // GET CUSTOMER USER ID
        // -----------------------------------------------------

        var customer = await _context.Customers
            .FirstOrDefaultAsync(c =>
                c.Id == booking.CustomerId &&
                !c.IsDeleted);

        if (customer == null ||
            string.IsNullOrWhiteSpace(customer.UserId))
        {
            TempData["Error"] =
                "Customer account could not be found.";

            return RedirectToAction(nameof(Index));
        }


        // -----------------------------------------------------
        // UPDATE BOOKING STATUS
        // -----------------------------------------------------

        booking.Status = BookingStatus.Confirmed;

        await _bookingRepository.UpdateAsync(booking);


        // -----------------------------------------------------
        // CREATE NOTIFICATION
        // -----------------------------------------------------

        var notification = new Notification
        {
            UserId = customer.UserId,

            Title = "Booking Confirmed",

            Message =
                $"Your appointment on {booking.BookingDate:dd MMM yyyy} " +
                $"from {booking.StartTime:hh\\:mm} to {booking.EndTime:hh\\:mm} " +
                "has been confirmed.",

            BookingId = booking.Id,

            IsRead = false,

            CreatedAt = DateTime.Now
        };

        await _notificationRepository.AddAsync(notification);


        TempData["Success"] =
            "Booking confirmed successfully.";

        return RedirectToAction(nameof(Index));
    }


    // =========================================================
    // COMPLETE BOOKING
    // =========================================================

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CompleteBooking(int id)
    {
        var booking = await _bookingRepository.GetByIdAsync(id);

        if (booking == null)
            return NotFound();


        // Only confirmed bookings can be completed

        if (booking.Status != BookingStatus.Confirmed)
        {
            TempData["Error"] =
                "Only confirmed bookings can be completed.";

            return RedirectToAction(nameof(Index));
        }


        // -----------------------------------------------------
        // GET CUSTOMER USER ID
        // -----------------------------------------------------

        var customer = await _context.Customers
            .FirstOrDefaultAsync(c =>
                c.Id == booking.CustomerId &&
                !c.IsDeleted);

        if (customer == null ||
            string.IsNullOrWhiteSpace(customer.UserId))
        {
            TempData["Error"] =
                "Customer account could not be found.";

            return RedirectToAction(nameof(Index));
        }


        // -----------------------------------------------------
        // UPDATE BOOKING STATUS
        // -----------------------------------------------------

        booking.Status = BookingStatus.Completed;

        await _bookingRepository.UpdateAsync(booking);


        // -----------------------------------------------------
        // CREATE NOTIFICATION
        // -----------------------------------------------------

        var notification = new Notification
        {
            UserId = customer.UserId,

            Title = "Appointment Completed",

            Message =
                $"Your appointment on {booking.BookingDate:dd MMM yyyy} " +
                $"from {booking.StartTime:hh\\:mm} to {booking.EndTime:hh\\:mm} " +
                "has been completed.",

            BookingId = booking.Id,

            IsRead = false,

            CreatedAt = DateTime.Now
        };

        await _notificationRepository.AddAsync(notification);


        TempData["Success"] =
            "Booking completed successfully.";

        return RedirectToAction(nameof(Index));
    }


    // =========================================================
    // CANCEL BOOKING
    // =========================================================

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CancelBooking(int id)
    {
        var booking = await _bookingRepository.GetByIdAsync(id);

        if (booking == null)
            return NotFound();


        // Only Pending or Confirmed bookings can be cancelled

        if (booking.Status != BookingStatus.Pending &&
            booking.Status != BookingStatus.Confirmed)
        {
            TempData["Error"] =
                "This booking cannot be cancelled because it is already completed or cancelled.";

            return RedirectToAction(nameof(Index));
        }


        // -----------------------------------------------------
        // CHECK APPOINTMENT TIME
        // -----------------------------------------------------

        var appointmentDateTime =
            booking.BookingDate.ToDateTime(booking.StartTime);

        if (appointmentDateTime <= DateTime.Now)
        {
            TempData["Error"] =
                "This appointment can no longer be cancelled because the appointment time has passed.";

            return RedirectToAction(nameof(Index));
        }


        // -----------------------------------------------------
        // GET CUSTOMER USER ID
        // -----------------------------------------------------

        var customer = await _context.Customers
            .FirstOrDefaultAsync(c =>
                c.Id == booking.CustomerId &&
                !c.IsDeleted);

        if (customer == null ||
            string.IsNullOrWhiteSpace(customer.UserId))
        {
            TempData["Error"] =
                "Customer account could not be found.";

            return RedirectToAction(nameof(Index));
        }


        // -----------------------------------------------------
        // UPDATE BOOKING STATUS
        // -----------------------------------------------------

        booking.Status = BookingStatus.Cancelled;

        await _bookingRepository.UpdateAsync(booking);


        // -----------------------------------------------------
        // CREATE NOTIFICATION
        // -----------------------------------------------------

        var notification = new Notification
        {
            UserId = customer.UserId,

            Title = "Booking Cancelled",

            Message =
                $"Your appointment on {booking.BookingDate:dd MMM yyyy} " +
                $"from {booking.StartTime:hh\\:mm} to {booking.EndTime:hh\\:mm} " +
                "has been cancelled.",

            BookingId = booking.Id,

            IsRead = false,

            CreatedAt = DateTime.Now
        };

        await _notificationRepository.AddAsync(notification);


        TempData["Success"] =
            "Booking cancelled successfully.";

        return RedirectToAction(nameof(Index));
    }
}