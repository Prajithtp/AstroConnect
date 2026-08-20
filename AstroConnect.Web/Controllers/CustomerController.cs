using AstroConnect.Application.Interfaces.Repositories;
using AstroConnect.Domain.Common;
using AstroConnect.Domain.Entities;
using AstroConnect.Web.Models.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using AstroConnect.Web.ViewModels.Receptionist;

namespace AstroConnect.Web.Controllers;

[Authorize(Roles = Roles.Admin + "," + Roles.Receptionist)]
public class CustomerController : Controller
{
    private readonly ICustomerRepository _customerRepository;
    private readonly IBookingRepository _bookingRepository;

    public CustomerController(
        ICustomerRepository customerRepository,
        IBookingRepository bookingRepository)
    {
        _customerRepository = customerRepository;
        _bookingRepository = bookingRepository;
    }

    // =========================================================
    // CUSTOMER LIST
    // =========================================================

    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var customers = await _customerRepository.GetAllAsync();

        return View(customers);
    }



    // =========================================================
    // CUSTOMER DETAILS
    // =========================================================

    [HttpGet]
    public async Task<IActionResult> Details(int id)
    {
        var customer = await _customerRepository.GetByIdAsync(id);

        if (customer == null)
        {
            return NotFound();
        }

        var bookings = await _bookingRepository.GetByCustomerIdAsync(id);

        var model = new CustomerDetailsViewModel
        {
            Customer = customer,
            Bookings = bookings
                .Where(b => !b.IsDeleted)
                .OrderByDescending(b => b.BookingDate)
                .ThenByDescending(b => b.StartTime)
                .ToList()
        };

        return View(model);
    }


    // =========================================================
    // CREATE CUSTOMER
    // =========================================================

    [HttpGet]
    public IActionResult Create()
    {
        return View();
    }


    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CustomerCreateViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var customer = new Customer
        {
            FullName = model.FullName,
            Email = model.Email,
            PhoneNumber = model.PhoneNumber,
            DateOfBirth = model.DateOfBirth,
            Gender = model.Gender,
            BirthTime = model.BirthTime,
            BirthPlace = model.BirthPlace,
            Address = model.Address
        };

        await _customerRepository.AddAsync(customer);

        return RedirectToAction(nameof(Index));
    }


    // =========================================================
    // EDIT CUSTOMER
    // =========================================================

    [HttpGet]
    public async Task<IActionResult> Edit(int id)
    {
        var customer = await _customerRepository.GetByIdAsync(id);

        if (customer == null)
        {
            return NotFound();
        }

        var model = new CustomerEditViewModel
        {
            Id = customer.Id,
            FullName = customer.FullName,
            Email = customer.Email,
            PhoneNumber = customer.PhoneNumber,
            DateOfBirth = customer.DateOfBirth,
            Gender = customer.Gender,
            BirthTime = customer.BirthTime,
            BirthPlace = customer.BirthPlace,
            Address = customer.Address
        };

        return View(model);
    }


    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(CustomerEditViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var customer = await _customerRepository.GetByIdAsync(model.Id);

        if (customer == null)
        {
            return NotFound();
        }

        customer.FullName = model.FullName;
        customer.Email = model.Email;
        customer.PhoneNumber = model.PhoneNumber;
        customer.DateOfBirth = model.DateOfBirth;
        customer.Gender = model.Gender;
        customer.BirthTime = model.BirthTime;
        customer.BirthPlace = model.BirthPlace;
        customer.Address = model.Address;

        await _customerRepository.UpdateAsync(customer);

        return RedirectToAction(nameof(Index));
    }


    // =========================================================
    // DELETE CUSTOMER
    // =========================================================

    [HttpGet]
    public async Task<IActionResult> Delete(int id)
    {
        var customer = await _customerRepository.GetByIdAsync(id);

        if (customer == null)
        {
            return NotFound();
        }

        return View(customer);
    }


    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        await _customerRepository.DeleteAsync(id);

        return RedirectToAction(nameof(Index));
    }


    // =========================================================
    // GET CUSTOMER
    // =========================================================

    [HttpGet]
    public async Task<IActionResult> GetCustomer(int id)
    {
        var customer = await _customerRepository.GetByIdAsync(id);

        if (customer == null)
        {
            return NotFound();
        }

        return Json(new
        {
            customer.PhoneNumber,
            customer.Gender,
            customer.DateOfBirth,
            customer.BirthPlace
        });
    }
}