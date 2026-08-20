using AstroConnect.Persistence.Context;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AstroConnect.Web.Controllers
{
    public class PublicController : Controller
    {
        private readonly ApplicationDbContext _context;

        public PublicController(ApplicationDbContext context)
        {
            _context = context;
        }


        public IActionResult Home()
        {
            return View();
        }


        [HttpGet]
        public async Task<IActionResult> Services()
        {
            var services = await _context.Services
                .Where(s =>
                    !s.IsDeleted &&
                    s.IsActive)
                .OrderBy(s => s.Name)
                .ToListAsync();

            return View(services);
        }


        [HttpGet]
        public async Task<IActionResult> Astrologers()
        {
            var astrologers = await _context.Astrologers
                .Where(a =>
                    !a.IsDeleted &&
                    a.IsActive)
                .OrderBy(a => a.FullName)
                .ToListAsync();

            return View(astrologers);
        }


        public IActionResult About()
        {
            return View();
        }


        public IActionResult Contact()
        {
            return View();
        }
    }
}