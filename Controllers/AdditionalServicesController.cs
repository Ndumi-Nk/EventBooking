using EventBooking.Data;
using EventBooking.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EventBooking.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdditionalServicesController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _environment;

        public AdditionalServicesController(ApplicationDbContext context, IWebHostEnvironment environment)
        {
            _context = context;
            _environment = environment;
        }

        // GET: List all services
        public async Task<IActionResult> Index()
        {
            var services = await _context.AdditionalServices.ToListAsync();
            return View(services);
        }

        // GET: Details
        public async Task<IActionResult> Details(int id)
        {
            var service = await _context.AdditionalServices.FindAsync(id);
            if (service == null) return NotFound();
            return View(service);
        }

        // GET: Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(AdditionalService model, IFormFile? imageFile)
        {
            if (!ModelState.IsValid) return View(model);

            // Upload image
            if (imageFile != null && imageFile.Length > 0)
            {
                string uploadsFolder = Path.Combine(_environment.WebRootPath, "images", "services");
                Directory.CreateDirectory(uploadsFolder);

                string uniqueFileName = Guid.NewGuid() + Path.GetExtension(imageFile.FileName);
                string filePath = Path.Combine(uploadsFolder, uniqueFileName);

                using (var fs = new FileStream(filePath, FileMode.Create))
                {
                    await imageFile.CopyToAsync(fs);
                }

                model.ImagePath = "/images/services/" + uniqueFileName;
            }

            _context.AdditionalServices.Add(model);
            await _context.SaveChangesAsync();
            TempData["Message"] = "Service created successfully!";
            return RedirectToAction(nameof(Index));
        }

        // GET: Edit
        public async Task<IActionResult> Edit(int id)
        {
            var service = await _context.AdditionalServices.FindAsync(id);
            if (service == null) return NotFound();
            return View(service);
        }

        // POST: Edit
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, AdditionalService model, IFormFile? imageFile)
        {
            if (id != model.ServiceId) return NotFound();
            if (!ModelState.IsValid) return View(model);

            var service = await _context.AdditionalServices.FindAsync(id);
            if (service == null) return NotFound();

            // Update fields
            service.ServiceName = model.ServiceName;
            service.Description = model.Description;
            service.Price = model.Price;
            service.PriceType = model.PriceType;
            service.IsActive = model.IsActive;

            // Replace image if new one uploaded
            if (imageFile != null && imageFile.Length > 0)
            {
                string uploadsFolder = Path.Combine(_environment.WebRootPath, "images", "services");
                Directory.CreateDirectory(uploadsFolder);

                string uniqueFileName = Guid.NewGuid() + Path.GetExtension(imageFile.FileName);
                string filePath = Path.Combine(uploadsFolder, uniqueFileName);

                using (var fs = new FileStream(filePath, FileMode.Create))
                {
                    await imageFile.CopyToAsync(fs);
                }

                // Optionally delete old file
                if (!string.IsNullOrEmpty(service.ImagePath))
                {
                    var oldFile = Path.Combine(_environment.WebRootPath, service.ImagePath.TrimStart('/'));
                    if (System.IO.File.Exists(oldFile)) System.IO.File.Delete(oldFile);
                }

                service.ImagePath = "/images/services/" + uniqueFileName;
            }

            _context.Update(service);
            await _context.SaveChangesAsync();

            TempData["Message"] = "Service updated successfully!";
            return RedirectToAction(nameof(Index));
        }

        // GET: Delete confirm
        public async Task<IActionResult> Delete(int id)
        {
            var service = await _context.AdditionalServices.FindAsync(id);
            if (service == null) return NotFound();
            return View(service);
        }

        // POST: Delete
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var service = await _context.AdditionalServices.FindAsync(id);
            if (service == null) return NotFound();

            // Delete image file if exists
            if (!string.IsNullOrEmpty(service.ImagePath))
            {
                var oldFile = Path.Combine(_environment.WebRootPath, service.ImagePath.TrimStart('/'));
                if (System.IO.File.Exists(oldFile)) System.IO.File.Delete(oldFile);
            }

            _context.AdditionalServices.Remove(service);
            await _context.SaveChangesAsync();

            TempData["Message"] = "Service deleted successfully!";
            return RedirectToAction(nameof(Index));
        }
    }
}
