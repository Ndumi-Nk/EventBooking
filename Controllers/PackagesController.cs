using EventBooking.Data;
using EventBooking.Models;
using EventBooking.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EventBooking.Controllers
{
   
    public class PackagesController : Controller
    {
        private readonly ApplicationDbContext _context;

        public PackagesController(ApplicationDbContext context)
        {
            _context = context;
        }

        // ---------------- List All Packages ----------------
        public async Task<IActionResult> Index()
        {
            var packages = await _context.Packages
                .Include(p => p.Event)
                .Include(p => p.PackageCaterings)
                    .ThenInclude(pc => pc.CateringMenu)
                .Include(p => p.PackageServices)
                    .ThenInclude(ps => ps.AdditionalService)
                .ToListAsync();
            return View(packages);
        }

        [HttpGet]
        public async Task<IActionResult> Create()
        {
            var model = new PackageViewModel
            {
                Events = await _context.Events.Where(e => e.IsActive).ToListAsync(),
                CateringMenus = await _context.CateringMenus.Where(c => c.IsActive).ToListAsync(),
                AdditionalServices = await _context.AdditionalServices.Where(s => s.IsActive).ToListAsync()
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(PackageViewModel model)
        {
            // Re-populate dropdowns if validation fails
            if (!ModelState.IsValid)
            {
                model.Events = await _context.Events.Where(e => e.IsActive).ToListAsync();
                model.CateringMenus = await _context.CateringMenus.Where(c => c.IsActive).ToListAsync();
                model.AdditionalServices = await _context.AdditionalServices.Where(s => s.IsActive).ToListAsync();
                return View(model);
            }

            // Handle image upload
            string? imagePath = null;
            if (model.ImageFile != null && model.ImageFile.Length > 0)
            {
                var uploads = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images/packages");
                if (!Directory.Exists(uploads))
                    Directory.CreateDirectory(uploads);

                var fileName = $"{Guid.NewGuid()}_{Path.GetFileName(model.ImageFile.FileName)}";
                var filePath = Path.Combine(uploads, fileName);
                using var stream = new FileStream(filePath, FileMode.Create);
                await model.ImageFile.CopyToAsync(stream);
                imagePath = $"/images/packages/{fileName}";
            }

            var package = new Package
            {
                PackageName = model.PackageName,
                Description = model.Description,
                EventId = model.EventId,
                ImagePath = imagePath,
                TotalPrice = model.TotalPrice,
                CreatedAt = DateTime.Now,
                PackageCaterings = new List<PackageCatering>(),
                PackageServices = new List<PackageService>()
            };

            // Add selected caterings
            if (model.SelectedCateringIds != null)
            {
                foreach (var menuId in model.SelectedCateringIds)
                {
                    var menu = await _context.CateringMenus.FindAsync(menuId);
                    if (menu != null)
                    {
                        package.PackageCaterings.Add(new PackageCatering
                        {
                            CateringMenu = menu,
                            Package = package
                        });
                    }
                }
            }

            // Add selected services
            if (model.SelectedServiceIds != null)
            {
                foreach (var serviceId in model.SelectedServiceIds)
                {
                    var service = await _context.AdditionalServices.FindAsync(serviceId);
                    if (service != null)
                    {
                        package.PackageServices.Add(new PackageService
                        {
                            AdditionalService = service,
                            Package = package
                        });
                    }
                }
            }

            _context.Packages.Add(package);
            await _context.SaveChangesAsync();

            TempData["Message"] = "Package created successfully!";
            return RedirectToAction(nameof(Index));
        }




        // ---------------- Details ----------------
        public async Task<IActionResult> Details(int id)
        {
            var package = await _context.Packages
                .Include(p => p.Event)
                .Include(p => p.PackageCaterings)
                    .ThenInclude(pc => pc.CateringMenu)
                .Include(p => p.PackageServices)
                    .ThenInclude(ps => ps.AdditionalService)
                .FirstOrDefaultAsync(p => p.PackageId == id);

            if (package == null) return NotFound();

            return View(package);
        }

        // ---------------- Delete Package ----------------
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var package = await _context.Packages.FindAsync(id);
            if (package == null) return NotFound();

            _context.Packages.Remove(package);
            await _context.SaveChangesAsync();
            TempData["Message"] = "Package deleted successfully!";
            return RedirectToAction(nameof(Index));
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteAll()
        {
            // Load all packages including related entities
            var packages = await _context.Packages
                .Include(p => p.PackageCaterings)
                .Include(p => p.PackageServices)
                .ToListAsync();

            // Remove all related many-to-many entries first
            foreach (var package in packages)
            {
                _context.PackageCaterings.RemoveRange(package.PackageCaterings);
                _context.PackageServices.RemoveRange(package.PackageServices);
            }

            // Then remove the packages themselves
            _context.Packages.RemoveRange(packages);

            await _context.SaveChangesAsync();

            TempData["Message"] = "All packages deleted successfully!";
            return RedirectToAction(nameof(Index));
        }

    }
}
