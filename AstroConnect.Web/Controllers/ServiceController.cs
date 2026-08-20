using AstroConnect.Application.Interfaces.Repositories;
using AstroConnect.Domain.Common;
using AstroConnect.Domain.Entities;
using AstroConnect.Web.Models;
using AstroConnect.Web.ViewModels.Service;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AstroConnect.Web.Controllers
{
    [Authorize(Roles = Roles.Admin)]
    public class ServiceController : Controller
    {
        private readonly IServiceRepository _serviceRepository;

        public ServiceController(IServiceRepository serviceRepository)
        {
            _serviceRepository = serviceRepository;
        }

        public async Task<IActionResult> Index()
        {
            var services = await _serviceRepository.GetAllAsync();
            return View(services);
        }
        public IActionResult Create()
        {
            var model = new ServiceCreateViewModel();

            return View(model);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ServiceCreateViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var service = new Service
            {
                Name = model.Name,
                Description = model.Description,
                Price = model.Price,
                DurationInMinutes = model.DurationInMinutes,
                IsActive = model.IsActive
            };

            await _serviceRepository.AddAsync(service);

            return RedirectToAction(nameof(Index));
        }
        public async Task<IActionResult> Edit(int id)
        {
            var service = await _serviceRepository.GetByIdAsync(id);

            if (service == null)
            {
                return NotFound();
            }

            var model = new ServiceEditViewModel
            {
                Id = service.Id,
                Name = service.Name,
                Description = service.Description,
                Price = service.Price,
                DurationInMinutes = service.DurationInMinutes
            };

            return View(model);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(ServiceEditViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var service = await _serviceRepository.GetByIdAsync(model.Id);

            if (service == null)
            {
                return NotFound();
            }

            service.Name = model.Name;
            service.Description = model.Description;
            service.Price = model.Price;
            service.DurationInMinutes = model.DurationInMinutes;

            await _serviceRepository.UpdateAsync(service);

            return RedirectToAction(nameof(Index));
        }
        public async Task<IActionResult> Delete(int id)
        {
            var service = await _serviceRepository.GetByIdAsync(id);

            if (service == null)
            {
                return NotFound();
            }

            return View(service);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            await _serviceRepository.DeleteAsync(id);

            return RedirectToAction(nameof(Index));
        }
        [HttpGet]
        public async Task<IActionResult> GetService(int id)
        {
            var service = await _serviceRepository.GetByIdAsync(id);

            if (service == null)
            {
                return NotFound();
            }

            return Json(new
            {
                service.DurationInMinutes
            });
        }
    }

}