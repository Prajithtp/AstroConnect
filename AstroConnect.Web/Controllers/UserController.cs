using AstroConnect.Domain.Common;
using AstroConnect.Persistence.Identity;
using AstroConnect.Web.ViewModels.User;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AstroConnect.Web.Controllers;

[Authorize(Roles = Roles.Admin)]
public class UserController : Controller
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly RoleManager<IdentityRole> _roleManager;

    public UserController(
        UserManager<ApplicationUser> userManager,
        RoleManager<IdentityRole> roleManager)
    {
        _userManager = userManager;
        _roleManager = roleManager;
    }

    // =========================================================
    // USER LIST
    // =========================================================

    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var users = await _userManager.Users
            .OrderBy(u => u.FullName)
            .ToListAsync();

        var model = new List<UserListViewModel>();

        foreach (var user in users)
        {
            var roles = await _userManager.GetRolesAsync(user);

            model.Add(new UserListViewModel
            {
                Id = user.Id,
                FullName = user.FullName,
                Email = user.Email ?? string.Empty,
                PhoneNumber = user.PhoneNumber,
                Role = roles.FirstOrDefault() ?? "No Role",
                EmailConfirmed = user.EmailConfirmed
            });
        }

        return View(model);
    }


    // =========================================================
    // CREATE USER - GET
    // =========================================================

    [HttpGet]
    public IActionResult Create()
    {
        return View();
    }


    // =========================================================
    // CREATE USER - POST
    // =========================================================

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(UserCreateViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        if (!await _roleManager.RoleExistsAsync(model.Role))
        {
            ModelState.AddModelError(
                nameof(model.Role),
                "The selected role does not exist.");

            return View(model);
        }

        var user = new ApplicationUser
        {
            UserName = model.Email,
            Email = model.Email,
            FullName = model.FullName,
            EmailConfirmed = true
        };

        var result = await _userManager.CreateAsync(
            user,
            model.Password);

        if (result.Succeeded)
        {
            await _userManager.AddToRoleAsync(
                user,
                model.Role);

            return RedirectToAction(nameof(Index));
        }

        foreach (var error in result.Errors)
        {
            ModelState.AddModelError(
                string.Empty,
                error.Description);
        }

        return View(model);
    }


    // =========================================================
    // EDIT USER - GET
    // =========================================================

    [HttpGet]
    public async Task<IActionResult> Edit(string id)
    {
        if (string.IsNullOrEmpty(id))
        {
            return NotFound();
        }

        var user = await _userManager.FindByIdAsync(id);

        if (user == null)
        {
            return NotFound();
        }

        var roles = await _userManager.GetRolesAsync(user);

        var model = new UserEditViewModel
        {
            Id = user.Id,
            FullName = user.FullName,
            Email = user.Email ?? string.Empty,
            PhoneNumber = user.PhoneNumber,
            Role = roles.FirstOrDefault() ?? string.Empty
        };

        return View(model);
    }


    // =========================================================
    // EDIT USER - POST
    // =========================================================

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(UserEditViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        if (!await _roleManager.RoleExistsAsync(model.Role))
        {
            ModelState.AddModelError(
                nameof(model.Role),
                "The selected role does not exist.");

            return View(model);
        }

        var user = await _userManager.FindByIdAsync(model.Id);

        if (user == null)
        {
            return NotFound();
        }

        var currentRoles = await _userManager.GetRolesAsync(user);

        // =====================================================
        // PROTECT ALL ADMIN ACCOUNTS
        // =====================================================

        if (currentRoles.Contains(Roles.Admin))
        {
            // Admin role cannot be changed
            if (model.Role != Roles.Admin)
            {
                ModelState.AddModelError(
                    nameof(model.Role),
                    "Admin accounts must remain in the Admin role.");

                return View(model);
            }
        }

        // =====================================================
        // UPDATE USER INFORMATION
        // =====================================================

        user.FullName = model.FullName;
        user.Email = model.Email;
        user.UserName = model.Email;
        user.PhoneNumber = model.PhoneNumber;

        var updateResult = await _userManager.UpdateAsync(user);

        if (!updateResult.Succeeded)
        {
            foreach (var error in updateResult.Errors)
            {
                ModelState.AddModelError(
                    string.Empty,
                    error.Description);
            }

            return View(model);
        }

        // =====================================================
        // UPDATE ROLE
        // =====================================================

        currentRoles = await _userManager.GetRolesAsync(user);

        if (currentRoles.Any())
        {
            var removeResult =
                await _userManager.RemoveFromRolesAsync(
                    user,
                    currentRoles);

            if (!removeResult.Succeeded)
            {
                foreach (var error in removeResult.Errors)
                {
                    ModelState.AddModelError(
                        string.Empty,
                        error.Description);
                }

                return View(model);
            }
        }

        var addRoleResult =
            await _userManager.AddToRoleAsync(
                user,
                model.Role);

        if (!addRoleResult.Succeeded)
        {
            foreach (var error in addRoleResult.Errors)
            {
                ModelState.AddModelError(
                    string.Empty,
                    error.Description);
            }

            return View(model);
        }

        return RedirectToAction(nameof(Index));
    }


    // =========================================================
    // DELETE USER - GET
    // =========================================================

    [HttpGet]
    public async Task<IActionResult> Delete(string id)
    {
        if (string.IsNullOrWhiteSpace(id))
        {
            return NotFound();
        }

        var user = await _userManager.FindByIdAsync(id);

        if (user == null)
        {
            return NotFound();
        }

        var roles = await _userManager.GetRolesAsync(user);

        // Admin accounts cannot be deleted
        if (roles.Contains(Roles.Admin))
        {
            TempData["Error"] =
                "Admin accounts cannot be deleted.";

            return RedirectToAction(nameof(Index));
        }

        var model = new UserDeleteViewModel
        {
            Id = user.Id,
            FullName = user.FullName,
            Email = user.Email ?? string.Empty
        };

        return View(model);
    }


    // =========================================================
    // DELETE USER - POST
    // =========================================================

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(UserDeleteViewModel model)
    {
        var user = await _userManager.FindByIdAsync(model.Id);

        if (user == null)
        {
            return NotFound();
        }

        var roles = await _userManager.GetRolesAsync(user);

        // Admin accounts cannot be deleted
        if (roles.Contains(Roles.Admin))
        {
            TempData["Error"] =
                "Admin accounts cannot be deleted.";

            return RedirectToAction(nameof(Index));
        }

        var result = await _userManager.DeleteAsync(user);

        if (!result.Succeeded)
        {
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(
                    string.Empty,
                    error.Description);
            }

            return View(model);
        }

        return RedirectToAction(nameof(Index));
    }
}