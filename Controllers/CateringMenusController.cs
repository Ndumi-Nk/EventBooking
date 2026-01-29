using EventBooking.Data;
using EventBooking.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.IO;
using System.Linq;

namespace EventBooking.Controllers
{

    [Route("[controller]/[action]")]
    public class CateringMenusController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _environment;

        public CateringMenusController(ApplicationDbContext context, IWebHostEnvironment environment)
        {
            _context = context;
            _environment = environment;
        }

        // GET: CateringMenus
        public IActionResult Index()
        {
            return View(_context.CateringMenus.ToList());
        }

        // GET: CateringMenus/Details/5
        public IActionResult Details(int? id)
        {
            if (id == null) return NotFound();

            var menu = _context.CateringMenus.FirstOrDefault(m => m.MenuId == id);
            if (menu == null) return NotFound();

            return View(menu);
        }

        // GET: CateringMenus/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: CateringMenus/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(CateringMenu menu, IFormFile imageFile)
        {
            if (ModelState.IsValid)
            {
                if (imageFile != null && imageFile.Length > 0)
                {
                    var fileName = Guid.NewGuid() + Path.GetExtension(imageFile.FileName);
                    var uploadDir = Path.Combine(_environment.WebRootPath, "uploads/cateringmenus");

                    if (!Directory.Exists(uploadDir))
                        Directory.CreateDirectory(uploadDir);

                    var filePath = Path.Combine(uploadDir, fileName);
                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        imageFile.CopyTo(stream);
                    }

                    menu.ImagePath = "/uploads/cateringmenus/" + fileName;
                }

                _context.CateringMenus.Add(menu);
                _context.SaveChanges();
                return RedirectToAction(nameof(Index));
            }
            return View(menu);
        }

        // GET: CateringMenus/Edit/5
        public IActionResult Edit(int? id)
        {
            if (id == null) return NotFound();

            var menu = _context.CateringMenus.Find(id);
            if (menu == null) return NotFound();

            return View(menu);
        }

        // POST: CateringMenus/Edit
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(int id, CateringMenu menu, IFormFile imageFile)
        {
            if (id != menu.MenuId) return NotFound();

            if (ModelState.IsValid)
            {
                if (imageFile != null && imageFile.Length > 0)
                {
                    var fileName = Guid.NewGuid() + Path.GetExtension(imageFile.FileName);
                    var uploadDir = Path.Combine(_environment.WebRootPath, "uploads/cateringmenus");

                    if (!Directory.Exists(uploadDir))
                        Directory.CreateDirectory(uploadDir);

                    var filePath = Path.Combine(uploadDir, fileName);
                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        imageFile.CopyTo(stream);
                    }

                    menu.ImagePath = "/uploads/cateringmenus/" + fileName;
                }

                _context.Update(menu);
                _context.SaveChanges();
                return RedirectToAction(nameof(Index));
            }
            return View(menu);
        }

        // GET: CateringMenus/Delete/5
        public IActionResult Delete(int? id)
        {
            if (id == null) return NotFound();

            var menu = _context.CateringMenus.Find(id);
            if (menu == null) return NotFound();

            return View(menu);
        }

        // POST: CateringMenus/Delete
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteConfirmed(int id)
        {
            var menu = _context.CateringMenus.Find(id);
            if (menu != null)
            {
                _context.CateringMenus.Remove(menu);
                _context.SaveChanges();
            }
            return RedirectToAction(nameof(Index));
        }
    }
}
