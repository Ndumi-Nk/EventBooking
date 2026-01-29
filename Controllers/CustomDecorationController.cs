using EventBooking.Data;
using EventBooking.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace EventBooking.Controllers
{
    [Authorize]
    public class CustomDecorationController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public CustomDecorationController(ApplicationDbContext context, IWebHostEnvironment webHostEnvironment)
        {
            _context = context;
            _webHostEnvironment = webHostEnvironment;
        }

        // ---------------- List decorations of logged-in user ----------------
        public async Task<IActionResult> Index()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var decorations = await _context.CustomDecorations
                                            .Where(d => d.UserId == userId)
                                            .ToListAsync();
            return View(decorations);
        }

        // ---------------- Add new decoration ----------------
        public IActionResult Create()
        {
            return View(new CustomDecoration());
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CustomDecoration model, IFormFile ImageFile)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            // Validate image type and size
            if (ImageFile != null && ImageFile.Length > 0)
            {
                var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif" };
                var ext = Path.GetExtension(ImageFile.FileName).ToLower();

                if (!allowedExtensions.Contains(ext))
                {
                    ModelState.AddModelError("ImageFile", "Only JPG, PNG, and GIF files are allowed.");
                    return View(model);
                }

                if (ImageFile.Length > 5 * 1024 * 1024) // 5 MB limit
                {
                    ModelState.AddModelError("ImageFile", "File size must be under 5MB.");
                    return View(model);
                }

                // Ensure folder exists
                var uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "images/decorations");
                Directory.CreateDirectory(uploadsFolder);

                var fileName = Guid.NewGuid().ToString() + ext;
                var filePath = Path.Combine(uploadsFolder, fileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await ImageFile.CopyToAsync(stream);
                }

                model.ImageUrl = "/images/decorations/" + fileName;
            }

            // Get user ID from Claims
            model.UserId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            _context.CustomDecorations.Add(model);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Decoration added successfully!";
            return RedirectToAction(nameof(Index));
        }

        // ---------------- Edit decoration ----------------
        public async Task<IActionResult> Edit(int id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var decoration = await _context.CustomDecorations.FirstOrDefaultAsync(d => d.DecorationId == id && d.UserId == userId);
            if (decoration == null) return NotFound();

            return View(decoration);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, CustomDecoration model, IFormFile ImageFile)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var decoration = await _context.CustomDecorations.FirstOrDefaultAsync(d => d.DecorationId == id && d.UserId == userId);
            if (decoration == null) return NotFound();

            if (!ModelState.IsValid)
                return View(model);

            decoration.Name = model.Name;
            decoration.Description = model.Description;
            decoration.Price = model.Price;

            if (ImageFile != null && ImageFile.Length > 0)
            {
                var uploads = Path.Combine(_webHostEnvironment.WebRootPath, "images/decorations");
                Directory.CreateDirectory(uploads);
                var fileName = Guid.NewGuid() + Path.GetExtension(ImageFile.FileName);
                var filePath = Path.Combine(uploads, fileName);
                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    await ImageFile.CopyToAsync(fileStream);
                }
                decoration.ImageUrl = "/images/decorations/" + fileName;
            }

            _context.CustomDecorations.Update(decoration);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Decoration updated successfully!";
            return RedirectToAction(nameof(Index));
        }

        // ---------------- Delete decoration ----------------
        public async Task<IActionResult> Delete(int id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var decoration = await _context.CustomDecorations.FirstOrDefaultAsync(d => d.DecorationId == id && d.UserId == userId);
            if (decoration == null) return NotFound();

            return View(decoration);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var decoration = await _context.CustomDecorations.FirstOrDefaultAsync(d => d.DecorationId == id && d.UserId == userId);
            if (decoration == null) return NotFound();

            _context.CustomDecorations.Remove(decoration);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Decoration deleted successfully!";
            return RedirectToAction(nameof(Index));
        }
    }
}
