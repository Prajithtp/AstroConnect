using AstroConnect.Application.Interfaces.Repositories;
using AstroConnect.Domain.Common;
using AstroConnect.Domain.Entities;
using AstroConnect.Web.Models.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;


namespace AstroConnect.Web.Controllers;

[Authorize(Roles = Roles.Admin)]
public class AstrologerController : Controller
{
    private readonly IAstrologerRepository _astrologerRepository;

    public AstrologerController(IAstrologerRepository astrologerRepository)
    {
        _astrologerRepository = astrologerRepository;
    }

    public async Task<IActionResult> Index()
    {
        var astrologers = await _astrologerRepository.GetAllAsync();
        return View(astrologers);
    }
    [Authorize(Roles = Roles.Admin)]
    [HttpGet]
    public IActionResult Create()
    {
        return View();
    }
    [Authorize(Roles = Roles.Admin)]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(AstrologerCreateViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var astrologer = new AstroConnect.Domain.Entities.Astrologer
        {
            FullName = model.FullName,
            Email = model.Email,
            PhoneNumber = model.PhoneNumber,
            Experience = model.Experience,
            Specialization = model.Specialization,
            Language = model.Language,
            Biography = model.Biography,
            ProfileImageUrl = model.ProfileImageUrl,
            IsActive = model.IsActive
        };

        await _astrologerRepository.AddAsync(astrologer);

        return RedirectToAction(nameof(Index));
    }
    [Authorize(Roles = Roles.Admin)]
    [HttpGet]
    public async Task<IActionResult> Edit(int id)
    {
        var astrologer = await _astrologerRepository.GetByIdAsync(id);

        if (astrologer == null)
        {
            return NotFound();
        }

        var model = new AstrologerEditViewModel
        {
            Id = astrologer.Id,
            FullName = astrologer.FullName,
            Email = astrologer.Email,
            PhoneNumber = astrologer.PhoneNumber,
            Experience = astrologer.Experience,
            Specialization = astrologer.Specialization,
            Language = astrologer.Language,
            Biography = astrologer.Biography,
            ProfileImageUrl = astrologer.ProfileImageUrl,
            IsActive = astrologer.IsActive
        };

        return View(model);
    }
    [Authorize(Roles = Roles.Admin)]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(AstrologerEditViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var astrologer = await _astrologerRepository.GetByIdAsync(model.Id);

        if (astrologer == null)
        {
            return NotFound();
        }

        astrologer.FullName = model.FullName;
        astrologer.Email = model.Email;
        astrologer.PhoneNumber = model.PhoneNumber;
        astrologer.Experience = model.Experience;
        astrologer.Specialization = model.Specialization;
        astrologer.Language = model.Language;
        astrologer.Biography = model.Biography;
        astrologer.ProfileImageUrl = model.ProfileImageUrl;
        astrologer.IsActive = model.IsActive;

        await _astrologerRepository.UpdateAsync(astrologer);

        return RedirectToAction(nameof(Index));
    }
    [Authorize(Roles = Roles.Admin)]
    [HttpGet]
    public async Task<IActionResult> Delete(int id)
    {
        var astrologer = await _astrologerRepository.GetByIdAsync(id);

        if (astrologer == null)
        {
            return NotFound();
        }

        return View(astrologer);
    }
    [Authorize(Roles = Roles.Admin)]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        await _astrologerRepository.DeleteAsync(id);

        return RedirectToAction(nameof(Index));
    }
}