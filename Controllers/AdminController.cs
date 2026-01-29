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
    public class AdminController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _environment;

        public AdminController(ApplicationDbContext context, IWebHostEnvironment environment)
        {
            _context = context;
            _environment = environment;
        }

        public async Task<IActionResult> Dashboard()
        {
            var dashboard = new AdminDashboardViewModel
            {
                TotalEvents = await _context.Events.CountAsync(e => e.IsActive),
                TotalBookings = await _context.Bookings.CountAsync(),
                PendingApprovals = await _context.Bookings.CountAsync(b => b.Status == BookingStatus.Pending),
                TotalRevenue = await _context.Payments.Where(p => p.Status == PaymentStatus.Completed).SumAsync(p => p.Amount),
                RecentBookings = await _context.Bookings
                    .Include(b => b.User)
                    .Include(b => b.Event)
                    .OrderByDescending(b => b.BookingDate)
                    .Take(10)
                    .ToListAsync(),

                // Removed filtering and ordering by EventDate
                UpcomingEvents = await _context.Events
                    .Where(e => e.IsActive)
                    .Take(5)
                    .ToListAsync()
            };

            return View(dashboard);
        }


        public async Task<IActionResult> AllBookings()
        {
            // Get Event Bookings
            var eventBookings = await _context.Bookings
                .Include(b => b.Event)
                .Include(b => b.User)
                .Include(b => b.BookingCaterings)
                    .ThenInclude(c => c.CateringMenu)
                .Include(b => b.BookingServices)
                    .ThenInclude(s => s.AdditionalService)
                .Select(b => new UnifiedBookingViewModel
                {
                    BookingId = b.BookingId,
                    BookingType = "Event",
                    EventOrPackageName = b.Event.EventName,
                    UserName = b.User.FirstName + " " + b.User.LastName,
                    NumberOfPeople = b.NumberOfPeople,
                    Status = b.Status.ToString(),
                    BookingDate = b.BookingDate,
                    TotalAmount = b.TotalAmount,
                    EventStartTime = b.EventStartTime,
                    EventEndTime = b.EventEndTime
                })
                .ToListAsync();

            // Get Package Bookings
            var packageBookings = await (
                from b in _context.PackageBookings
                join u in _context.Users on b.UserId equals u.Id
                join p in _context.Packages on b.PackageId equals p.PackageId
                select new UnifiedBookingViewModel
                {
                    BookingId = b.PackageBookingId,
                    BookingType = "Package",
                    EventOrPackageName = p.PackageName,
                    UserName = u.FirstName + " " + u.LastName,
                    NumberOfPeople = b.NumberOfPeople,
                    Status = b.Status.ToString(),
                    BookingDate = b.CreatedAt,
                    TotalAmount = p.TotalPrice,
                    EventStartTime = null,
                    EventEndTime = null
                }
            ).ToListAsync();

            // Combine
            var allBookings = eventBookings.Concat(packageBookings)
                                           .OrderByDescending(b => b.BookingDate)
                                           .ToList();

            return View(allBookings);
        }


    }

        public class AdminDashboardViewModel
    {
        public int TotalEvents { get; set; }
        public int TotalBookings { get; set; }
        public int PendingApprovals { get; set; }
        public decimal TotalRevenue { get; set; }
        public List<Booking> RecentBookings { get; set; }
        public List<Event> UpcomingEvents { get; set; }
    }
}
