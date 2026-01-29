using EventBooking.Data;
using EventBooking.Models;
using EventBooking.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;

namespace EventBooking.Controllers
{
    public class HomeController : Controller
    {
        private readonly ApplicationDbContext _context;

        public HomeController(ApplicationDbContext context)
        {
            _context = context;
        }
       
        public async Task<IActionResult> Index()
        {
            var activeEvents = await _context.Events
                .Where(e => e.IsActive)
                .Take(6)
                .ToListAsync();

            ViewBag.FeaturedEvents = activeEvents.Take(3).ToList();
            return View(activeEvents);
        }

        public async Task<IActionResult> Events()
        {
            var events = await _context.Events
                .Where(e => e.IsActive)
                .ToListAsync();

            return View(events);
        }

        public async Task<IActionResult> EventDetails(int id)
        {
            var eventItem = await _context.Events
                .FirstOrDefaultAsync(e => e.EventId == id && e.IsActive);

            if (eventItem == null)
            {
                return NotFound();
            }

            return View(eventItem);
        }

        public IActionResult About()
        {
            return View();
        }

        [HttpGet]
        public IActionResult Contact()
        {
            return View(new ContactViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult SendMessage(ContactViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View("Contact", model);
            }

            // TODO: Send email or save to database
            // Example: _emailService.SendContactEmail(model);

            TempData["Success"] = "Your message has been sent successfully!";
            return RedirectToAction("Contact");
        }
    }
}

