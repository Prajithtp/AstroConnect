using AstroConnect.Application.Interfaces.Repositories;
using AstroConnect.Domain.Common;
using AstroConnect.Domain.Entities;
using AstroConnect.Domain.Enums;
using AstroConnect.Persistence.Context;
using AstroConnect.Persistence.Identity;
using AstroConnect.Persistence.Repositories;
using AstroConnect.Web.ViewModels.Booking;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace AstroConnect.Web.Controllers;

// Keep [Authorize] because Customer, Receptionist and Admin
// can use the booking functionality.
[Authorize]
public class BookingController : Controller
{
    private readonly IBookingRepository _bookingRepository;
    private readonly ICustomerRepository _customerRepository;
    private readonly IServiceRepository _serviceRepository;
    private readonly IAstrologerRepository _astrologerRepository;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ApplicationDbContext _context;
    private readonly INotificationRepository _notificationRepository;

    // =========================================================
    // BOOKING SETTINGS
    // =========================================================

    // All AstroConnect services currently use 30 minutes.
    private const int AppointmentDurationMinutes = 30;

    // Business hours
    private static readonly TimeOnly OpeningTime = new(9, 0);
    private static readonly TimeOnly ClosingTime = new(18, 0);


    public BookingController(
        IBookingRepository bookingRepository,
        ICustomerRepository customerRepository,
        IServiceRepository serviceRepository,
        IAstrologerRepository astrologerRepository,
        INotificationRepository notificationRepository,
        UserManager<ApplicationUser> userManager,
        ApplicationDbContext context)
    {
        _bookingRepository = bookingRepository;
        _customerRepository = customerRepository;
        _serviceRepository = serviceRepository;
        _astrologerRepository = astrologerRepository;
        _userManager = userManager;
        _context = context;
        _notificationRepository = notificationRepository;
    }


    // =========================================================
    // INDEX
    // =========================================================
    [Authorize(Roles = Roles.Admin)]
    public async Task<IActionResult> Index()
    {
        var bookings = await _bookingRepository.GetAllAsync();

        return View(bookings);
    }


    // =========================================================
    // CREATE - GET
    // =========================================================

    [HttpGet]
    public async Task<IActionResult> Create(int? astrologerId)
    {
        // Only these three roles can create bookings.
        if (!User.IsInRole(Roles.Admin) &&
            !User.IsInRole(Roles.Receptionist) &&
            !User.IsInRole(Roles.Customer))
        {
            return Forbid();
        }

        var model = new BookingCreateViewModel();

        // Get logged-in user
        var user = await _userManager.GetUserAsync(User);

        if (user == null)
        {
            return RedirectToAction("Login", "Account");
        }


        // =====================================================
        // CUSTOMER
        // =====================================================

        if (User.IsInRole(Roles.Customer))
        {
            var customer = await _context.Customers
                .FirstOrDefaultAsync(c =>
                    c.UserId == user.Id &&
                    !c.IsDeleted);

            if (customer == null)
            {
                return NotFound(
                    "Customer profile was not found.");
            }

            // Automatically use logged-in customer's ID.
            model.CustomerId = customer.Id;

            // Customer does not need to select themselves.
            model.Customers = new List<SelectListItem>
        {
            new SelectListItem
            {
                Value = customer.Id.ToString(),
                Text = customer.FullName,
                Selected = true
            }
        };
        }
        else
        {
            // Admin / Receptionist can select customer.
            model.Customers =
                (await _customerRepository.GetAllAsync())
                .Select(c => new SelectListItem
                {
                    Value = c.Id.ToString(),
                    Text = c.FullName
                })
                .ToList();
        }


        // =====================================================
        // ASTROLOGERS
        // =====================================================

        model.Astrologers =
            (await _astrologerRepository.GetAllAsync())
            .Where(a => a.IsActive)
            .Select(a => new SelectListItem
            {
                Value = a.Id.ToString(),
                Text = a.FullName,
                Selected = astrologerId.HasValue &&
                           a.Id == astrologerId.Value
            })
            .ToList();


        // =====================================================
        // SERVICES
        // =====================================================

        model.Services =
            (await _serviceRepository.GetAllAsync())
            .Select(s => new SelectListItem
            {
                Value = s.Id.ToString(),
                Text = s.Name
            })
            .ToList();


        return View(model);
    }


    // =========================================================
    // CREATE - POST
    // =========================================================

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(
        BookingCreateViewModel model)
    {
        // =====================================================
        // ROLE CHECK
        // =====================================================

        if (!User.IsInRole(Roles.Admin) &&
            !User.IsInRole(Roles.Receptionist) &&
            !User.IsInRole(Roles.Customer))
        {
            return Forbid();
        }


        // =====================================================
        // CUSTOMER
        // =====================================================

        if (User.IsInRole(Roles.Customer))
        {
            var user = await _userManager.GetUserAsync(User);

            if (user == null)
            {
                return RedirectToAction(
                    "Login",
                    "Account");
            }

            var customer = await _context.Customers
                .FirstOrDefaultAsync(c =>
                    c.UserId == user.Id &&
                    !c.IsDeleted);

            if (customer == null)
            {
                TempData["Error"] =
                    "Customer profile was not found. Please complete your profile first.";

                return RedirectToAction(
                    "Profile",
                    "CustomerPortal");
            }

            // NEVER trust CustomerId from browser.
            model.CustomerId = customer.Id;
        }


        // =====================================================
        // GET SELECTED SERVICE
        // =====================================================

        var service = await _context.Services
            .FirstOrDefaultAsync(s =>
                s.Id == model.ServiceId &&
                !s.IsDeleted &&
                s.IsActive);

        if (service == null)
        {
            ModelState.AddModelError(
                nameof(model.ServiceId),
                "Please select a valid service.");

            await LoadDropdowns(model);

            return View(model);
        }

        var astrologer = await _astrologerRepository
    .GetByIdAsync(model.AstrologerId);

        if (astrologer == null)
        {
            ModelState.AddModelError(
                nameof(model.AstrologerId),
                "Please select a valid astrologer.");
        }
        else if (!astrologer.IsActive)
        {
            ModelState.AddModelError(
                nameof(model.AstrologerId),
                "The selected astrologer is currently unavailable for booking.");
        }

        // =====================================================
        // BOOKING DATE VALIDATION
        // =====================================================

        var today = DateOnly.FromDateTime(
            DateTime.Today);

        if (model.BookingDate < today)
        {
            ModelState.AddModelError(
                nameof(model.BookingDate),
                "Booking date cannot be in the past.");
        }


        // =====================================================
        // TIME VALIDATION
        // =====================================================

        var expectedEndTime =
            model.StartTime.AddMinutes(
                AppointmentDurationMinutes);


        // Start time must be before end time.
        if (model.StartTime >= model.EndTime)
        {
            ModelState.AddModelError(
                nameof(model.StartTime),
                "Start time must be before end time.");
        }


        // The appointment must be exactly 30 minutes.
        if (model.EndTime != expectedEndTime)
        {
            ModelState.AddModelError(
                nameof(model.EndTime),
                "Appointments must be exactly 30 minutes.");
        }


        // Appointment must start within business hours.
        if (model.StartTime < OpeningTime)
        {
            ModelState.AddModelError(
                nameof(model.StartTime),
                "Appointments cannot start before 9:00 AM.");
        }


        // Appointment must finish by 6:00 PM.
        if (model.EndTime > ClosingTime)
        {
            ModelState.AddModelError(
                nameof(model.EndTime),
                "Appointments must finish by 6:00 PM.");
        }


        // =====================================================
        // 30-MINUTE SLOT ALIGNMENT
        // =====================================================

        if (!IsValidThirtyMinuteSlot(model.StartTime))
        {
            ModelState.AddModelError(
                nameof(model.StartTime),
                "Please select a valid 30-minute appointment slot.");
        }


        // =====================================================
        // TODAY'S PAST TIME VALIDATION
        // =====================================================

        if (model.BookingDate == today)
        {
            var currentTime =
                TimeOnly.FromDateTime(DateTime.Now);

            if (model.StartTime <= currentTime)
            {
                ModelState.AddModelError(
                    nameof(model.StartTime),
                    "This appointment time has already passed. Please select a future time.");
            }
        }


        // =====================================================
        // MODEL VALIDATION
        // =====================================================

        if (!ModelState.IsValid)
        {
            await LoadDropdowns(model);

            return View(model);
        }


        // =====================================================
        // CHECK ASTROLOGER SLOT
        // =====================================================

        bool available =
            await _bookingRepository
                .IsTimeSlotAvailableAsync(
                    model.AstrologerId,
                    model.BookingDate,
                    model.StartTime,
                    model.EndTime);

        if (!available)
        {
            ModelState.AddModelError(
                "",
                "This astrologer already has a booking during the selected time.");

            await LoadDropdowns(model);

            return View(model);
        }


        // =====================================================
        // CREATE BOOKING
        // =====================================================

        var booking = new Booking
        {
            CustomerId = model.CustomerId,

            AstrologerId = model.AstrologerId,

            ServiceId = model.ServiceId,

            BookingDate = model.BookingDate,

            StartTime = model.StartTime,

            EndTime = model.EndTime,

            WhatsAppNumber = model.WhatsAppNumber,

            Gender = model.Gender,

            BirthDate = model.BirthDate,

            BirthTime = model.BirthTime,

            BirthPlace = model.BirthPlace,

            Question = model.Question
        };


        await _bookingRepository.AddAsync(booking);


        // =====================================================
        // NOTIFY RECEPTIONIST
        // =====================================================

        if (User.IsInRole(Roles.Customer))
        {
            var receptionistRoleUsers =
                await _userManager.GetUsersInRoleAsync(Roles.Receptionist);

            foreach (var receptionist in receptionistRoleUsers)
            {
                var notification = new Notification
                {
                    UserId = receptionist.Id,

                    Title = "New Booking Received",

                    Message =
                        $"A new appointment has been booked for " +
                        $"{booking.BookingDate:dd MMM yyyy} " +
                        $"from {booking.StartTime:hh\\:mm} to {booking.EndTime:hh\\:mm}.",

                    BookingId = booking.Id,

                    IsRead = false,

                    CreatedAt = DateTime.Now
                };

                await _notificationRepository.AddAsync(notification);
            }
        }


        // =====================================================
        // AFTER BOOKING
        // =====================================================

        if (User.IsInRole(Roles.Customer))
        {
            TempData["Success"] =
                "Appointment booked successfully!";

            return RedirectToAction(
                "Index",
                "CustomerPortal");
        }


        return RedirectToAction(nameof(Index));
    }


    // =========================================================
    // LOAD DROPDOWNS
    // =========================================================

    private async Task LoadDropdowns(
        BookingCreateViewModel model)
    {
        // =====================================================
        // CUSTOMERS
        // =====================================================

        if (!User.IsInRole(Roles.Customer))
        {
            model.Customers =
                (await _customerRepository.GetAllAsync())
                .Select(c => new SelectListItem
                {
                    Value = c.Id.ToString(),

                    Text = c.FullName,

                    Selected =
                        c.Id == model.CustomerId
                })
                .ToList();
        }


        // =====================================================
        // ASTROLOGERS
        // =====================================================

        model.Astrologers =
    (await _astrologerRepository.GetAllAsync())
    .Where(a => a.IsActive)
    .Select(a => new SelectListItem
    {
        Value = a.Id.ToString(),
        Text = a.FullName,
        Selected = a.Id == model.AstrologerId
    })
    .ToList();


        // =====================================================
        // SERVICES
        // =====================================================

        model.Services =
            (await _serviceRepository.GetAllAsync())
            .Select(s => new SelectListItem
            {
                Value = s.Id.ToString(),

                Text = s.Name,

                Selected =
                    s.Id == model.ServiceId
            })
            .ToList();
    }


    // =========================================================
    // CHECK 30-MINUTE SLOT
    // =========================================================

    private static bool IsValidThirtyMinuteSlot(
        TimeOnly startTime)
    {
        // Valid starts:
        //
        // 09:00
        // 09:30
        // 10:00
        // 10:30
        // ...
        // 17:30

        if (startTime < OpeningTime ||
            startTime >= ClosingTime)
        {
            return false;
        }

        int minutesFromOpening =
            (int)(
                startTime.ToTimeSpan() -
                OpeningTime.ToTimeSpan())
            .TotalMinutes;

        return minutesFromOpening % 30 == 0;
    }


    // =========================================================
    // DETAILS
    // =========================================================
    [Authorize(Roles = Roles.Admin)]
    public async Task<IActionResult> Details(int id)
    {
        var booking =
            await _bookingRepository.GetByIdAsync(id);

        if (booking == null)
        {
            return NotFound();
        }

        return View(booking);
    }


    // =========================================================
    // EDIT - GET
    // =========================================================
    [Authorize(Roles = Roles.Admin)]
    [HttpGet]
    public async Task<IActionResult> Edit(int id)
    {
        var booking =
            await _bookingRepository.GetByIdAsync(id);

        if (booking == null)
        {
            return NotFound();
        }


        var model = new BookingCreateViewModel
        {
            CustomerId = booking.CustomerId,

            AstrologerId = booking.AstrologerId,

            ServiceId = booking.ServiceId,

            BookingDate = booking.BookingDate,

            StartTime = booking.StartTime,

            EndTime = booking.EndTime,

            WhatsAppNumber =
                booking.WhatsAppNumber,

            Gender = booking.Gender,

            BirthDate = booking.BirthDate,

            BirthTime = booking.BirthTime,

            BirthPlace = booking.BirthPlace,

            Question = booking.Question
        };


        await LoadDropdowns(model);


        return View(model);
    }


    // =========================================================
    // EDIT - POST
    // =========================================================
    [Authorize(Roles = Roles.Admin)]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(
        int id,
        BookingCreateViewModel model)
    {
        // =====================================================
        // CUSTOMER
        // =====================================================

        if (User.IsInRole(Roles.Customer))
        {
            var user =
                await _userManager.GetUserAsync(User);

            if (user == null)
            {
                return RedirectToAction(
                    "Login",
                    "Account");
            }


            var customer =
                await _context.Customers
                    .FirstOrDefaultAsync(c =>
                        c.UserId == user.Id &&
                        !c.IsDeleted);


            if (customer == null)
            {
                return NotFound();
            }


            // Never trust CustomerId
            // from the browser.
            model.CustomerId = customer.Id;
        }

        // -----------------------------------------------------
        // CHECK ASTROLOGER AVAILABILITY
        // -----------------------------------------------------

        var astrologer = await _astrologerRepository
            .GetByIdAsync(model.AstrologerId);

        if (astrologer == null)
        {
            ModelState.AddModelError(
                nameof(model.AstrologerId),
                "Please select a valid astrologer.");
        }
        else if (!astrologer.IsActive)
        {
            ModelState.AddModelError(
                nameof(model.AstrologerId),
                "The selected astrologer is currently unavailable for booking.");
        }


        // =====================================================
        // GET EXISTING BOOKING
        // =====================================================

        var booking =
            await _bookingRepository.GetByIdAsync(id);

        if (booking == null)
        {
            return NotFound();
        }


        // =====================================================
        // GET SERVICE
        // =====================================================

        var service =
            await _context.Services
                .FirstOrDefaultAsync(s =>
                    s.Id == model.ServiceId &&
                    !s.IsDeleted &&
                    s.IsActive);


        if (service == null)
        {
            ModelState.AddModelError(
                nameof(model.ServiceId),
                "Please select a valid service.");
        }


        // =====================================================
        // DATE VALIDATION
        // =====================================================

        var today =
            DateOnly.FromDateTime(DateTime.Today);


        if (model.BookingDate < today)
        {
            ModelState.AddModelError(
                nameof(model.BookingDate),
                "Booking date cannot be in the past.");
        }


        // =====================================================
        // TIME VALIDATION
        // =====================================================

        var expectedEndTime =
            model.StartTime.AddMinutes(
                AppointmentDurationMinutes);


        if (model.StartTime >= model.EndTime)
        {
            ModelState.AddModelError(
                nameof(model.StartTime),
                "Start time must be before end time.");
        }


        if (model.EndTime != expectedEndTime)
        {
            ModelState.AddModelError(
                nameof(model.EndTime),
                "Appointments must be exactly 30 minutes.");
        }


        if (model.StartTime < OpeningTime)
        {
            ModelState.AddModelError(
                nameof(model.StartTime),
                "Appointments cannot start before 9:00 AM.");
        }


        if (model.EndTime > ClosingTime)
        {
            ModelState.AddModelError(
                nameof(model.EndTime),
                "Appointments must finish by 6:00 PM.");
        }


        // =====================================================
        // SLOT ALIGNMENT
        // =====================================================

        if (!IsValidThirtyMinuteSlot(model.StartTime))
        {
            ModelState.AddModelError(
                nameof(model.StartTime),
                "Please select a valid 30-minute appointment slot.");
        }


        // =====================================================
        // TODAY'S PAST TIME
        // =====================================================

        if (model.BookingDate == today)
        {
            var currentTime =
                TimeOnly.FromDateTime(DateTime.Now);

            if (model.StartTime <= currentTime)
            {
                ModelState.AddModelError(
                    nameof(model.StartTime),
                    "This appointment time has already passed. Please select a future time.");
            }
        }


        // =====================================================
        // MODEL VALIDATION
        // =====================================================

        if (!ModelState.IsValid)
        {
            await LoadDropdowns(model);

            return View(model);
        }


        // =====================================================
        // CHECK SLOT AVAILABILITY
        // =====================================================

        bool available =
            await _bookingRepository
                .IsTimeSlotAvailableForEditAsync(
                    id,
                    model.AstrologerId,
                    model.BookingDate,
                    model.StartTime,
                    model.EndTime);


        if (!available)
        {
            ModelState.AddModelError(
                "",
                "This astrologer already has a booking during the selected time.");

            await LoadDropdowns(model);

            return View(model);
        }


        // =====================================================
        // UPDATE BOOKING
        // =====================================================

        booking.CustomerId =
            model.CustomerId;

        booking.AstrologerId =
            model.AstrologerId;

        booking.ServiceId =
            model.ServiceId;

        booking.BookingDate =
            model.BookingDate;

        booking.StartTime =
            model.StartTime;

        booking.EndTime =
            model.EndTime;

        booking.WhatsAppNumber =
            model.WhatsAppNumber;

        booking.Gender =
            model.Gender;

        booking.BirthDate =
            model.BirthDate;

        booking.BirthTime =
            model.BirthTime;

        booking.BirthPlace =
            model.BirthPlace;

        booking.Question =
            model.Question;


        await _bookingRepository.UpdateAsync(
            booking);


        // =====================================================
        // AFTER UPDATE
        // =====================================================

        if (User.IsInRole(Roles.Customer))
        {
            TempData["Success"] =
                "Appointment updated successfully!";

            return RedirectToAction(
                "Index",
                "CustomerPortal");
        }


        return RedirectToAction(nameof(Index));
    }


    // =========================================================
    // DELETE - GET
    // =========================================================
    [Authorize(Roles = Roles.Admin)]
    [HttpGet]
    public async Task<IActionResult> Delete(int id)
    {
        var booking =
            await _bookingRepository.GetByIdAsync(id);

        if (booking == null)
        {
            return NotFound();
        }

        return View(booking);
    }


    // =========================================================
    // DELETE - POST
    // =========================================================
    [Authorize(Roles = Roles.Admin)]
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(
        int id)
    {
        await _bookingRepository.DeleteAsync(id);

        return RedirectToAction(nameof(Index));
    }


    // =========================================================
    // CHANGE BOOKING STATUS
    // =========================================================
    [Authorize(Roles = Roles.Admin)]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ChangeStatus(
        int id,
        BookingStatus status)
    {
        var booking =
            await _bookingRepository.GetByIdAsync(id);

        if (booking == null)
        {
            return NotFound();
        }


        var currentStatus =
            booking.Status;


        // =====================================================
        // STATUS TRANSITION RULES
        // =====================================================

        bool isValidTransition =
            currentStatus switch
            {
                // Pending → Confirmed OR Cancelled
                BookingStatus.Pending =>
                    status == BookingStatus.Confirmed ||
                    status == BookingStatus.Cancelled,

                // Confirmed → Completed OR Cancelled
                BookingStatus.Confirmed =>
                    status == BookingStatus.Completed ||
                    status == BookingStatus.Cancelled,

                // Completed → No changes
                BookingStatus.Completed =>
                    false,

                // Cancelled → No changes
                BookingStatus.Cancelled =>
                    false,

                _ => false
            };


        // =====================================================
        // INVALID TRANSITION
        // =====================================================

        if (!isValidTransition)
        {
            TempData["Error"] =
                $"Cannot change booking status from {currentStatus} to {status}.";

            return RedirectToAction(
                nameof(Index));
        }


        // =====================================================
        // UPDATE STATUS
        // =====================================================

        booking.Status = status;

        await _bookingRepository.UpdateAsync(
            booking);


        TempData["Success"] =
            $"Booking status changed from {currentStatus} to {status}.";


        return RedirectToAction(
            nameof(Index));
    }


    // =========================================================
    // GET SERVICE
    // =========================================================

    [HttpGet]
    public async Task<IActionResult> GetService(
        int id)
    {
        var service =
            await _context.Services
                .FirstOrDefaultAsync(s =>
                    s.Id == id &&
                    !s.IsDeleted &&
                    s.IsActive);


        if (service == null)
        {
            return NotFound();
        }


        return Json(new
        {
            id = service.Id,

            name = service.Name,

            // All current services use 30 minutes.
            durationInMinutes =
                AppointmentDurationMinutes,

            price = service.Price
        });
    }


    // =========================================================
    // GET AVAILABLE SLOTS
    // =========================================================

    [HttpGet]
    public async Task<IActionResult> GetAvailableSlots(
        int astrologerId,
        DateOnly bookingDate,
        int serviceId)
    {
        // =====================================================
        // GET SERVICE
        // =====================================================

        var service =
            await _context.Services
                .FirstOrDefaultAsync(s =>
                    s.Id == serviceId &&
                    !s.IsDeleted &&
                    s.IsActive);


        if (service == null)
        {
            return NotFound(
                "Service not found.");
        }


        // =====================================================
        // PAST DATE
        // =====================================================

        var today =
            DateOnly.FromDateTime(
                DateTime.Today);


        if (bookingDate < today)
        {
            return Json(
                new List<object>());
        }

        var astrologer = await _astrologerRepository
    .GetByIdAsync(astrologerId);

        if (astrologer == null || !astrologer.IsActive)
        {
            return Json(new List<object>());
        }


        // =====================================================
        // EXISTING BOOKINGS
        // =====================================================

        var existingBookings =
            await _context.Bookings
                .Where(b =>
                    !b.IsDeleted &&

                    b.AstrologerId ==
                        astrologerId &&

                    b.BookingDate ==
                        bookingDate &&

                    b.Status !=
                        BookingStatus.Cancelled)
                .Select(b => new
                {
                    b.StartTime,
                    b.EndTime
                })
                .ToListAsync();


        // =====================================================
        // DETERMINE FIRST SLOT
        // =====================================================

        var firstSlotTime =
            OpeningTime;


        if (bookingDate == today)
        {
            var currentTime =
                TimeOnly.FromDateTime(
                    DateTime.Now);


            // If business hours are over,
            // there are no slots today.
            if (currentTime >= ClosingTime)
            {
                return Json(
                    new List<object>());
            }


            // If current time is before opening,
            // start at 9:00 AM.
            if (currentTime <= OpeningTime)
            {
                firstSlotTime =
                    OpeningTime;
            }
            else
            {
                // =================================================
                // ROUND UP TO NEXT 30-MINUTE SLOT
                // =================================================
                //
                // Example:
                //
                // Current time = 1:37 PM
                //
                // Next slot = 2:00 PM
                //
                // Current time = 1:31 PM
                //
                // Next slot = 2:00 PM
                //
                // Current time = 1:30 PM
                //
                // Slot = 1:30 PM
                //

                var minutesFromOpening =
                    (int)(
                        currentTime.ToTimeSpan() -
                        OpeningTime.ToTimeSpan())
                    .TotalMinutes;


                var slotsPassed =
                    (minutesFromOpening + 29) / 30;


                firstSlotTime =
                    OpeningTime.AddMinutes(
                        slotsPassed * 30);
            }
        }


        // =====================================================
        // GENERATE SLOTS
        // =====================================================

        var slots =
            new List<object>();


        var currentSlot =
            firstSlotTime;


        while (
            currentSlot.AddMinutes(
                AppointmentDurationMinutes)
            <= ClosingTime)
        {
            var slotEnd =
                currentSlot.AddMinutes(
                    AppointmentDurationMinutes);


            // =================================================
            // CHECK EXISTING BOOKING CONFLICT
            // =================================================

            bool isAvailable =
                !existingBookings.Any(b =>
                    currentSlot < b.EndTime &&
                    slotEnd > b.StartTime);


            if (isAvailable)
            {
                slots.Add(new
                {
                    startTime =
                        currentSlot.ToString("HH:mm"),

                    endTime =
                        slotEnd.ToString("HH:mm"),

                    display =
                        $"{currentSlot:hh:mm tt} - {slotEnd:hh:mm tt}"
                });
            }


            currentSlot =
                currentSlot.AddMinutes(
                    AppointmentDurationMinutes);
        }


        return Json(slots);
    }
}