using AstroConnect.Domain.Entities;
using AstroConnect.Persistence.Context;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AstroConnect.Web.Controllers;

[Authorize]
public class AstrologersController : Controller
{
    private readonly ApplicationDbContext _context;

    public AstrologersController(ApplicationDbContext context)
    {
        _context = context;
    }

    // =========================================================
    // ASTROLOGER LIST
    // =========================================================

    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var astrologers = await _context.Astrologers
            .Where(a =>
                !a.IsDeleted &&
                a.IsActive)
            .OrderBy(a => a.FullName)
            .ToListAsync();

        return View(astrologers);
    }


    // =========================================================
    // ASTROLOGER DETAILS
    // =========================================================

    [HttpGet]
    public async Task<IActionResult> Details(int id)
    {
        var astrologer = await _context.Astrologers
            .FirstOrDefaultAsync(a =>
                a.Id == id &&
                !a.IsDeleted &&
                a.IsActive);

        if (astrologer == null)
            return NotFound();

        return View(astrologer);
    }
}