using AstroConnect.Domain.Common;
using AstroConnect.Persistence.Identity;
using AstroConnect.Web.ViewModels.Account;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using AstroConnect.Persistence.Context;
using AstroConnect.Domain.Entities;
namespace AstroConnect.Web.Controllers;

public class AccountController : Controller
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly ApplicationDbContext _context;

    public AccountController(
    UserManager<ApplicationUser> userManager,
    SignInManager<ApplicationUser> signInManager,
    ApplicationDbContext context)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _context = context;
    }

    [HttpGet]
    public IActionResult Login()
    {
        return View();
    }
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var result = await _signInManager.PasswordSignInAsync(
            model.Email,
            model.Password,
            model.RememberMe,
            lockoutOnFailure: true);

        if (result.IsLockedOut)
        {
            ModelState.AddModelError(
                "",
                "Your account has been temporarily locked due to multiple failed login attempts. Please try again later.");

            return View(model);
        }

        if (!result.Succeeded)
        {
            ModelState.AddModelError(
                "",
                "Invalid email or password.");

            return View(model);
        }
        var user = await _userManager.FindByEmailAsync(model.Email);

        if (user == null)
        {
            await _signInManager.SignOutAsync();

            ModelState.AddModelError("", "User account was not found.");
            return View(model);
        }

        // =====================================================
        // ADMIN
        // =====================================================

        if (await _userManager.IsInRoleAsync(user, Roles.Admin))
        {
            return RedirectToAction("Index", "User");
        }

        // =====================================================
        // RECEPTIONIST
        // =====================================================

        if (await _userManager.IsInRoleAsync(user, Roles.Receptionist))
        {
            return RedirectToAction("Index", "Receptionist");
        }

        // =====================================================
        // CUSTOMER
        // =====================================================

        if (await _userManager.IsInRoleAsync(user, Roles.Customer))
        {
            return RedirectToAction("Index", "CustomerPortal");
        }

        // =====================================================
        // UNKNOWN ROLE
        // =====================================================

        await _signInManager.SignOutAsync();

        ModelState.AddModelError(
            "",
            "Your account does not have a valid role.");

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout()
    {
        await _signInManager.SignOutAsync();

        return RedirectToAction(nameof(Login));
    }

    [HttpGet]
    public IActionResult Register()
    {
        return View();
    }
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Register(RegisterViewModel model)
    {
        if (!ModelState.IsValid)
            return View(model);

        var user = new ApplicationUser
        {
            FullName = model.FullName,
            UserName = model.Email,
            Email = model.Email,
            PhoneNumber = model.PhoneNumber,
            EmailConfirmed = true
        };

        var result = await _userManager.CreateAsync(user, model.Password);

        if (result.Succeeded)
        {
            // Assign Customer role
            // Assign Customer role
            var roleResult =
                await _userManager.AddToRoleAsync(
                    user,
                    Roles.Customer);

            if (!roleResult.Succeeded)
            {
                // Remove the newly-created account so we don't leave
                // a user without the required Customer role.
                await _userManager.DeleteAsync(user);

                foreach (var error in roleResult.Errors)
                {
                    ModelState.AddModelError(
                        string.Empty,
                        error.Description);
                }

                return View(model);
            }

            // Create Customer record

            // Create Customer record
            var customer = new Customer
            {
                UserId = user.Id,
                FullName = model.FullName,
                Email = model.Email,
                PhoneNumber = model.PhoneNumber,

                // Default values (Customer can update later)
                DateOfBirth = DateTime.Now,
                Gender = "",
                BirthTime = "",
                BirthPlace = "",
                Address = ""
            };

            _context.Customers.Add(customer);

            await _context.SaveChangesAsync();

            // Login automatically
            await _signInManager.SignInAsync(user, false);

            return RedirectToAction("Index", "CustomerPortal");
        }

        foreach (var error in result.Errors)
        {
            ModelState.AddModelError("", error.Description);
        }

        return View(model);
    }
}