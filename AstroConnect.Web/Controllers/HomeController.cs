using System.Diagnostics;
using AstroConnect.Application.Interfaces.Repositories;
using AstroConnect.Web.Models;
using AstroConnect.Web.ViewModels.Home;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AstroConnect.Web.Controllers;

public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;
    private readonly ICustomerRepository _customerRepository;
    private readonly IAstrologerRepository _astrologerRepository;
    private readonly IServiceRepository _serviceRepository;
    private readonly IBookingRepository _bookingRepository;

    public HomeController(
        ILogger<HomeController> logger,
        ICustomerRepository customerRepository,
        IAstrologerRepository astrologerRepository,
        IServiceRepository serviceRepository,
        IBookingRepository bookingRepository)
    {
        _logger = logger;
        _customerRepository = customerRepository;
        _astrologerRepository = astrologerRepository;
        _serviceRepository = serviceRepository;
        _bookingRepository = bookingRepository;
    }


    // =========================================================
    // LEGACY / GENERAL DASHBOARD
    // =========================================================

    [Authorize]
    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var customers = await _customerRepository.GetAllAsync();
        var astrologers = await _astrologerRepository.GetAllAsync();
        var services = await _serviceRepository.GetAllAsync();
        var bookings = await _bookingRepository.GetAllAsync();

        var model = new DashboardViewModel
        {
            TotalCustomers = customers.Count,
            TotalAstrologers = astrologers.Count,
            TotalServices = services.Count,
            TotalBookings = bookings.Count
        };

        return View(model);
    }


    // =========================================================
    // PRIVACY
    // =========================================================

    [AllowAnonymous]
    [HttpGet]
    public IActionResult Privacy()
    {
        return View();
    }


    // =========================================================
    // ERROR PAGE
    // =========================================================

    [AllowAnonymous]
    [ResponseCache(
        Duration = 0,
        Location = ResponseCacheLocation.None,
        NoStore = true)]
    public IActionResult Error()
    {
        var requestId =
            Activity.Current?.Id ??
            HttpContext.TraceIdentifier;

        _logger.LogError(
            "Application error page displayed. RequestId: {RequestId}",
            requestId);

        var model = new ErrorViewModel
        {
            RequestId = requestId
        };

        return View(model);
    }
}