using EventBooking.Data;
using EventBooking.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace EventBooking.Controllers
{
    public class EventsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _environment;

        public EventsController(ApplicationDbContext context, IWebHostEnvironment environment)
        {
            _context = context;
            _environment = environment;
        }

        // GET: All Active Events for Users
        public async Task<IActionResult> Index()
        {
            var events = await _context.Events
                                       .Where(e => e.IsActive)
                                       .ToListAsync();
            return View(events);
        }

        // GET: Admin - Manage All Events
        [Authorize(Roles = "Admin,Staff")]
        public async Task<IActionResult> Manage()
        {
            var events = await _context.Events
                                       .ToListAsync();
            return View(events);
        }

        // GET: Admin - Create Event
        [Authorize(Roles = "Admin,Staff")]
        public IActionResult Create()
        {
            return View();
        }

        // POST: Admin - Create Event
        [HttpPost]
        [Authorize(Roles = "Admin,Staff")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Event eventModel, IFormFile eventImage)
        {
            if (ModelState.IsValid)
            {
                if (eventImage != null && eventImage.Length > 0)
                {
                    eventModel.ImagePath = await SaveImageAsync(eventImage, "events");
                }

                eventModel.IsActive = true;
                _context.Events.Add(eventModel);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "Event created successfully!";
                return RedirectToAction(nameof(Manage));
            }

            return View(eventModel);
        }

        // GET: Admin - Edit Event
        [Authorize(Roles = "Admin,Staff")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var eventModel = await _context.Events.FindAsync(id);
            if (eventModel == null) return NotFound();

            return View(eventModel);
        }

        // POST: Admin - Edit Event
        [HttpPost]
        [Authorize(Roles = "Admin,Staff")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Event eventModel, IFormFile eventImage)
        {
            if (id != eventModel.EventId) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    var existingEvent = await _context.Events.AsNoTracking()
                                              .FirstOrDefaultAsync(e => e.EventId == id);
                    if (existingEvent == null) return NotFound();

                    if (eventImage != null && eventImage.Length > 0)
                    {
                        if (!string.IsNullOrEmpty(existingEvent.ImagePath))
                            DeleteImage(existingEvent.ImagePath);

                        eventModel.ImagePath = await SaveImageAsync(eventImage, "events");
                    }
                    else
                    {
                        eventModel.ImagePath = existingEvent.ImagePath;
                    }

                    _context.Events.Update(eventModel);
                    await _context.SaveChangesAsync();

                    TempData["SuccessMessage"] = "Event updated successfully!";
                    return RedirectToAction(nameof(Manage));
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!EventExists(eventModel.EventId)) return NotFound();
                    throw;
                }
            }

            return View(eventModel);
        }

        // POST: Admin - Soft Delete Event
        [HttpPost]
        [Authorize(Roles = "Admin,Staff")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var eventModel = await _context.Events.FindAsync(id);
            if (eventModel != null)
            {
                eventModel.IsActive = false;
                _context.Events.Update(eventModel);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "Event deleted successfully!";
            }

            return RedirectToAction(nameof(Manage));
        }

        // Utility: Check if event exists
        private bool EventExists(int id) => _context.Events.Any(e => e.EventId == id);

        // Utility: Save uploaded image
        private async Task<string> SaveImageAsync(IFormFile image, string folder)
        {
            var uploadsFolder = Path.Combine(_environment.WebRootPath, "images", folder);
            if (!Directory.Exists(uploadsFolder))
                Directory.CreateDirectory(uploadsFolder);

            var uniqueFileName = $"{Guid.NewGuid()}_{Path.GetFileName(image.FileName)}";
            var filePath = Path.Combine(uploadsFolder, uniqueFileName);

            using (var fileStream = new FileStream(filePath, FileMode.Create))
            {
                await image.CopyToAsync(fileStream);
            }

            return $"/images/{folder}/{uniqueFileName}";
        }

        // Utility: Delete image from wwwroot
        private void DeleteImage(string imagePath)
        {
            if (!string.IsNullOrEmpty(imagePath))
            {
                var fullPath = Path.Combine(_environment.WebRootPath, imagePath.TrimStart('/').Replace("/", "\\"));
                if (System.IO.File.Exists(fullPath))
                    System.IO.File.Delete(fullPath);
            }
        }
    }
}
