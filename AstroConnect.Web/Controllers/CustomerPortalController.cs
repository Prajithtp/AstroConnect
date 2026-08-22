using AstroConnect.Application.Interfaces.Repositories;
using AstroConnect.Domain.Common;
using AstroConnect.Domain.Enums;
using AstroConnect.Domain.Entities;
using AstroConnect.Persistence.Context;
using AstroConnect.Persistence.Identity;
using AstroConnect.Web.ViewModels.CustomerPortal;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AstroConnect.Web.Controllers;

[Authorize(Roles = Roles.Customer)]
public class CustomerPortalController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IBookingRepository _bookingRepository;
    private readonly INotificationRepository _notificationRepository;

    public CustomerPortalController(
        ApplicationDbContext context,
        UserManager<ApplicationUser> userManager,
        IBookingRepository bookingRepository,
        INotificationRepository notificationRepository)
    {
        _context = context;
        _userManager = userManager;
        _bookingRepository = bookingRepository;
        _notificationRepository = notificationRepository;
    }


    // =========================================================
    // CUSTOMER DASHBOARD
    // =========================================================

    [HttpGet]
    public async Task<IActionResult> Index()
    {
        // Get logged-in user
        var user = await _userManager.GetUserAsync(User);

        if (user == null)
        {
            return RedirectToAction("Login", "Account");
        }

        // Find customer profile
        var customer = await _context.Customers
            .FirstOrDefaultAsync(c =>
                c.UserId == user.Id &&
                !c.IsDeleted);

        if (customer == null)
        {
            return NotFound("Customer profile was not found.");
        }

        // Get customer's bookings
        var bookings = await _bookingRepository
            .GetByCustomerIdAsync(customer.Id);

        // Only active bookings
        var activeBookings = bookings
            .Where(b => !b.IsDeleted)
            .ToList();

        var now = DateTime.Now;

        // Upcoming bookings
        var upcomingBookings = activeBookings
            .Where(b =>
                b.Status == BookingStatus.Pending ||
                b.Status == BookingStatus.Confirmed)
            .Where(b =>
                b.BookingDate.ToDateTime(b.StartTime) > now)
            .OrderBy(b => b.BookingDate)
            .ThenBy(b => b.StartTime)
            .ToList();

        // Recent bookings
        var recentBookings = activeBookings
            .OrderByDescending(b => b.BookingDate)
            .ThenByDescending(b => b.StartTime)
            .Take(5)
            .ToList();

        // Get unread notification count
        var unreadNotificationCount = await _context.Set<Notification>()
            .CountAsync(n =>
                n.UserId == user.Id &&
                !n.IsRead);

        var model = new CustomerDashboardViewModel
        {
            // Booking statistics
            TotalBookings = activeBookings.Count,

            PendingBookings = activeBookings.Count(b =>
                b.Status == BookingStatus.Pending),

            ConfirmedBookings = activeBookings.Count(b =>
                b.Status == BookingStatus.Confirmed),

            CompletedBookings = activeBookings.Count(b =>
                b.Status == BookingStatus.Completed),

            // Next appointment
            UpcomingBooking = upcomingBookings.FirstOrDefault(),

            // Recent bookings
            RecentBookings = recentBookings,

            // Notifications
            UnreadNotificationCount = unreadNotificationCount
        };

        return View(model);
    }

    // =========================================================
    // CUSTOMER PROFILE - GET
    // =========================================================

    [HttpGet]
    public async Task<IActionResult> Profile()
    {
        var user = await _userManager.GetUserAsync(User);

        if (user == null)
            return RedirectToAction("Login", "Account");

        var customer = await _context.Customers
            .FirstOrDefaultAsync(c => c.UserId == user.Id);

        if (customer == null)
            return NotFound();

        var model = new CustomerProfileViewModel
        {
            CustomerId = customer.Id,
            FullName = customer.FullName,
            Email = customer.Email,
            PhoneNumber = customer.PhoneNumber,
            BirthDate = customer.DateOfBirth,

            BirthTime = string.IsNullOrEmpty(customer.BirthTime)
                ? TimeSpan.Zero
                : TimeSpan.Parse(customer.BirthTime),

            BirthPlace = customer.BirthPlace,
            Gender = customer.Gender,
            Address = customer.Address
        };

        return View(model);
    }


    // =========================================================
    // CUSTOMER PROFILE - POST
    // =========================================================

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Profile(CustomerProfileViewModel model)
    {
        var user = await _userManager.GetUserAsync(User);

        if (user == null)
            return RedirectToAction("Login", "Account");

        var customer = await _context.Customers
            .FirstOrDefaultAsync(c =>
                c.UserId == user.Id &&
                !c.IsDeleted);

        if (customer == null)
            return NotFound();


        // -----------------------------------------------------
        // EMAIL IS READ-ONLY ON CUSTOMER PROFILE
        // -----------------------------------------------------

        // Always use the email stored in the database.
        // This prevents a read-only/disabled email field from
        // causing Required validation to fail.

        model.Email = customer.Email;

        ModelState.Remove(nameof(model.Email));


        // -----------------------------------------------------
        // VALIDATION
        // -----------------------------------------------------

        if (!ModelState.IsValid)
        {
            model.CustomerId = customer.Id;

            return View(model);
        }


        // -----------------------------------------------------
        // UPDATE CUSTOMER PROFILE
        // -----------------------------------------------------

        customer.FullName = model.FullName;
        customer.PhoneNumber = model.PhoneNumber;
        customer.DateOfBirth = model.BirthDate;

        customer.BirthTime =
            model.BirthTime.ToString(@"hh\:mm");

        customer.BirthPlace = model.BirthPlace;
        customer.Gender = model.Gender;
        customer.Address = model.Address;


        // -----------------------------------------------------
        // UPDATE IDENTITY USER
        // -----------------------------------------------------

        user.FullName = model.FullName;
        user.PhoneNumber = model.PhoneNumber;


        // -----------------------------------------------------
        // SAVE
        // -----------------------------------------------------

        _context.Customers.Update(customer);

        var identityResult =
            await _userManager.UpdateAsync(user);

        if (!identityResult.Succeeded)
        {
            foreach (var error in identityResult.Errors)
            {
                ModelState.AddModelError(
                    string.Empty,
                    error.Description);
            }

            model.CustomerId = customer.Id;

            return View(model);
        }

        await _context.SaveChangesAsync();


        // -----------------------------------------------------
        // SUCCESS
        // -----------------------------------------------------

        TempData["Success"] =
            "Profile updated successfully.";

        return RedirectToAction(nameof(Profile));
    }


    // =========================================================
    // CUSTOMER BOOKING HISTORY
    // =========================================================

    [HttpGet]
    public async Task<IActionResult> BookingHistory()
    {
        var user = await _userManager.GetUserAsync(User);

        if (user == null)
            return RedirectToAction("Login", "Account");

        var customer = await _context.Customers
            .FirstOrDefaultAsync(c => c.UserId == user.Id);

        if (customer == null)
            return NotFound();

        var bookings = await _bookingRepository
            .GetByCustomerIdAsync(customer.Id);

        return View(bookings);
    }


    // =========================================================
    // CUSTOMER BOOKING DETAILS
    // =========================================================

    [HttpGet]
    public async Task<IActionResult> BookingDetails(int id)
    {
        var user = await _userManager.GetUserAsync(User);

        if (user == null)
            return RedirectToAction("Login", "Account");

        var customer = await _context.Customers
            .FirstOrDefaultAsync(c => c.UserId == user.Id);

        if (customer == null)
            return NotFound();

        var booking = await _bookingRepository.GetByIdAsync(id);

        if (booking == null)
            return NotFound();

        // Customer can only view their own booking
        if (booking.CustomerId != customer.Id)
            return Forbid();

        return View(booking);
    }


    // =========================================================
    // CUSTOMER CANCEL APPOINTMENT
    // =========================================================

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CancelBooking(int id)
    {
        var user = await _userManager.GetUserAsync(User);

        if (user == null)
            return RedirectToAction("Login", "Account");

        var customer = await _context.Customers
            .FirstOrDefaultAsync(c => c.UserId == user.Id);

        if (customer == null)
            return NotFound();

        var booking = await _bookingRepository.GetByIdAsync(id);

        if (booking == null)
            return NotFound();

        // -----------------------------------------------------
        // SECURITY CHECK
        // -----------------------------------------------------

        // Customer can only cancel their own booking
        if (booking.CustomerId != customer.Id)
            return Forbid();


        // -----------------------------------------------------
        // STATUS CHECK
        // -----------------------------------------------------

        // Only Pending bookings can be cancelled
        if (booking.Status != BookingStatus.Pending)
        {
            TempData["Error"] =
                "This appointment cannot be cancelled because it is already confirmed, completed, or cancelled.";

            return RedirectToAction(
                nameof(BookingDetails),
                new { id });
        }


        // -----------------------------------------------------
        // APPOINTMENT TIME CHECK
        // -----------------------------------------------------

        var appointmentDateTime =
            booking.BookingDate.ToDateTime(booking.StartTime);

        if (appointmentDateTime <= DateTime.Now)
        {
            TempData["Error"] =
                "This appointment can no longer be cancelled because the appointment time has passed.";

            return RedirectToAction(
                nameof(BookingDetails),
                new { id });
        }


        // -----------------------------------------------------
        // CHANGE BOOKING STATUS
        // -----------------------------------------------------

        booking.Status = BookingStatus.Cancelled;

        await _bookingRepository.UpdateAsync(booking);


        // -----------------------------------------------------
        // CREATE CUSTOMER NOTIFICATION
        // -----------------------------------------------------

        // Check whether a cancellation notification already
        // exists for this booking to avoid duplicates.
        var existingNotification = await _context.Notifications
            .AnyAsync(n =>
                n.BookingId == booking.Id &&
                n.UserId == user.Id &&
                n.Title == "Booking Cancelled");

        if (!existingNotification)
        {
            var notification = new Notification
            {
                UserId = user.Id,

                Title = "Booking Cancelled",

                Message =
                    $"Your appointment on {booking.BookingDate:dd MMM yyyy} " +
                    $"from {booking.StartTime:hh\\:mm} to {booking.EndTime:hh\\:mm} " +
                    "has been cancelled successfully.",

                BookingId = booking.Id,

                IsRead = false,

                CreatedAt = DateTime.Now
            };

            await _notificationRepository.AddAsync(notification);
        }


        // -----------------------------------------------------
        // SUCCESS MESSAGE
        // -----------------------------------------------------

        TempData["Success"] =
            "Your appointment has been cancelled successfully.";

        return RedirectToAction(
            nameof(BookingDetails),
            new { id });
    }
}   