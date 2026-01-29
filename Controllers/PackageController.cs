using EventBooking.Data;
using EventBooking.Models;
using EventBooking.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.Net;
using System.Net.Mail;
using System.Security.Claims;

namespace EventBooking.Controllers
{
  
    public class PackageController : Controller
    {


        private readonly ApplicationDbContext _context;

        public PackageController(ApplicationDbContext context)
        {
            _context = context;
        }
        [AllowAnonymous]
        public async Task<IActionResult> Browse()
        {
            var packages = await _context.Packages
     .Include(p => p.Event)
     .Include(p => p.PackageCaterings)
         .ThenInclude(pc => pc.CateringMenu)
     .Include(p => p.PackageServices)
         .ThenInclude(ps => ps.AdditionalService)
     .Where(p => p.Event.IsActive)
     .ToListAsync();


            return View(packages);
        }

        // ---------------- Client: View Package Details ----------------
        [AllowAnonymous]
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

        [HttpGet]
        // GET: Package/Book/5
        public async Task<IActionResult> Book(int id)
        {
            var package = await _context.Packages
                                        .Include(p => p.Event)
                                        .FirstOrDefaultAsync(p => p.PackageId == id);

            if (package == null)
                return NotFound();

            return View(package);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Book(PackageBookingViewModel model)
        {
            if (!ModelState.IsValid)
            {
                var package = await _context.Packages
                    .Include(p => p.Event)
                    .FirstOrDefaultAsync(p => p.PackageId == model.PackageId);

                if (package == null)
                    return NotFound();

                return View(package);
            }

            // Validate BookingDate
            if (model.BookingDate.Date < DateTime.Today)
            {
                ModelState.AddModelError(nameof(model.BookingDate), "Booking date cannot be in the past.");
                var package = await _context.Packages
                    .Include(p => p.Event)
                    .FirstOrDefaultAsync(p => p.PackageId == model.PackageId);
                return View(package);
            }

            // Capture logged-in user's ID
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null)
            {
                TempData["Error"] = "You must be logged in to book a package.";
                return RedirectToAction("Login", "Account");
            }

            var booking = new PackageBooking
            {
                PackageId = model.PackageId,
                NumberOfPeople = model.NumberOfPeople,
                BookingDate = model.BookingDate,
                CreatedAt = DateTime.Now,
                ContactName = model.ContactName,
                ContactEmail = model.ContactEmail,
                ContactPhone = model.ContactPhone,
                SpecialRequests = model.SpecialRequests,
                UserId = userId,
                Status = BookingStatuss.Pending
            };

            _context.PackageBookings.Add(booking);
            await _context.SaveChangesAsync();

            // --- SEND EMAIL VIA SMTP ---
            try
            {
                using (var smtp = new System.Net.Mail.SmtpClient("smtp.gmail.com", 587))
                {
                    smtp.Credentials = new System.Net.NetworkCredential("kwazi9939@gmail.com", "gido oabp fcgg idwc");
                    smtp.EnableSsl = true;

                    var mail = new System.Net.Mail.MailMessage
                    {
                        From = new System.Net.Mail.MailAddress("kwazi9939@gmail.com", "Event Booking System"),
                        Subject = $"Package Booking Confirmation - {booking.Package?.PackageName}",
                        IsBodyHtml = true,
                        Body = $@"
                    Dear {model.ContactName},<br/><br/>
                    Your booking for package <b>{booking.Package?.PackageName}</b> has been successfully created.<br/>
                    Booking Date: {booking.BookingDate:MMM dd, yyyy}<br/>
                    Number of People: {booking.NumberOfPeople}<br/>
                    Status: {booking.Status}<br/><br/>
                    Thank you for booking with us!
                "
                    };

                    mail.To.Add(model.ContactEmail);
                    await smtp.SendMailAsync(mail);
                }
            }
            catch (Exception ex)
            {
                // Log SMTP error if necessary
                Console.WriteLine("SMTP Error: " + ex.Message);
            }

            // Redirect to Booking Summary
            return RedirectToAction("Summary", new { id = booking.PackageBookingId });
        }

        [HttpGet]
        public async Task<IActionResult> Summary(int id)
        {
            var booking = await _context.PackageBookings
                                        .Include(b => b.Package)
                                            .ThenInclude(p => p.Event)
                                        .Include(b => b.Package)
                                            .ThenInclude(p => p.PackageCaterings)
                                                .ThenInclude(pc => pc.CateringMenu)
                                        .Include(b => b.Package)
                                            .ThenInclude(p => p.PackageServices)
                                                .ThenInclude(ps => ps.AdditionalService)
                                        .FirstOrDefaultAsync(b => b.PackageBookingId == id);

            if (booking == null)
                return NotFound();

            // Ensure the user can only see their own bookings
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (booking.UserId != userId)
                return Forbid();

            return View(booking);
        }
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> ManageBookings()
        {
            var bookings = await _context.PackageBookings
                .Include(b => b.Package)
                    .ThenInclude(p => p.Event)
                .OrderByDescending(b => b.CreatedAt)
                .ToListAsync();

            return View(bookings);
        }

        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DetailsBooking(int id)
        {
            var booking = await _context.PackageBookings
                .Include(b => b.Package)
                    .ThenInclude(p => p.Event)
                .Include(b => b.Package)
                    .ThenInclude(p => p.PackageCaterings)
                        .ThenInclude(pc => pc.CateringMenu)
                .Include(b => b.Package)
                    .ThenInclude(p => p.PackageServices)
                        .ThenInclude(ps => ps.AdditionalService)
                .FirstOrDefaultAsync(b => b.PackageBookingId == id);

            if (booking == null) return NotFound();

            return View(booking);
        }

        [Authorize] // Must be logged in
        public async Task<IActionResult> MyBookings()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var bookings = await _context.PackageBookings
                .Include(b => b.Package)
                    .ThenInclude(p => p.Event)
                .Where(b => b.UserId == userId)   // Only show logged-in user's bookings
                .OrderByDescending(b => b.CreatedAt)
                .ToListAsync();

            return View(bookings);
        }

      

// ---------------- ADMIN: Update Booking Status ----------------
[HttpPost]
    [Authorize(Roles = "Admin")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UpdateStatus(int id, BookingStatuss status)
    {
        var booking = await _context.PackageBookings
            .Include(b => b.Package)
            .FirstOrDefaultAsync(b => b.PackageBookingId == id);

        if (booking == null) return NotFound();

        booking.Status = status;
        _context.PackageBookings.Update(booking);
        await _context.SaveChangesAsync();

        TempData["Success"] = $"Booking has been marked as {status}.";

        // Send Email Notification to User
        var user = await _context.Users.FindAsync(booking.UserId);
        if (user != null)
        {
            try
            {
                using (var smtp = new SmtpClient("smtp.gmail.com", 587))
                {
                    smtp.Credentials = new NetworkCredential("kwazi9939@gmail.com", "gido oabp fcgg idwc");
                    smtp.EnableSsl = true;

                    var mail = new MailMessage
                    {
                        From = new MailAddress("kwazi9939@gmail.com", "Event Booking System"),
                        Subject = $"Booking Status Updated - {booking.Package?.PackageName}",
                        IsBodyHtml = true,
                        Body = $@"
                        Hi {user.FirstName},<br/><br/>
                        Your booking for <b>{booking.Package?.PackageName}</b> has been  <b>{status}</b>.<br/>
                        Booking Date: {booking.BookingDate:MMM dd, yyyy}<br/><br/>
                        Please <a href='https://eventbooking20251003061726-efewajd4h0c5atf5.southafricanorth-01.azurewebsites.net/Account/Login'>login</a> and make the payment to reserve your spot.<br/><br/>
                        Thank you,<br/>
                        Event Booking Team"
                    };
                    mail.To.Add(user.Email);

                    await smtp.SendMailAsync(mail);
                }
            }
            catch (Exception ex)
            {
                // Optionally log the error
                Console.WriteLine("SMTP Error: " + ex.Message);
            }
        }

        return RedirectToAction(nameof(ManageBookings));
    }

    // ---------------- USER / ADMIN: Cancel Booking ----------------
    [HttpPost]
    [Authorize]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CancelBooking(int id)
    {
        var booking = await _context.PackageBookings
            .Include(b => b.Package)
            .FirstOrDefaultAsync(b => b.PackageBookingId == id);

        if (booking == null) return NotFound();

        var userId = User.FindFirstValue(System.Security.Claims.ClaimTypes.NameIdentifier);

        if (booking.UserId != userId && !User.IsInRole("Admin"))
            return Forbid();

        booking.Status = BookingStatuss.Cancelled;
        _context.PackageBookings.Update(booking);
        await _context.SaveChangesAsync();

        TempData["Success"] = "Your booking has been cancelled.";

        // Send Email Notification
        var user = await _context.Users.FindAsync(booking.UserId);
        if (user != null)
        {
            try
            {
                using (var smtp = new SmtpClient("smtp.gmail.com", 587))
                {
                    smtp.Credentials = new NetworkCredential("kwazi9939@gmail.com", "gido oabp fcgg idwc");
                    smtp.EnableSsl = true;

                    var mail = new MailMessage
                    {
                        From = new MailAddress("kwazi9939@gmail.com", "Event Booking System"),
                        Subject = $"Booking Cancelled - {booking.Package?.PackageName}",
                        IsBodyHtml = true,
                        Body = $@"
                        Hi {user.FirstName},<br/><br/>
                        Your booking for <b>{booking.Package?.PackageName}</b> has been cancelled.<br/>
                        Booking Date: {booking.BookingDate:MMM dd, yyyy}<br/><br/>
                        If you have any questions, please contact support.<br/><br/>
                        Event Booking Team"
                    };
                    mail.To.Add(user.Email);

                    await smtp.SendMailAsync(mail);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("SMTP Error: " + ex.Message);
            }
        }

        if (User.IsInRole("Admin"))
            return RedirectToAction(nameof(ManageBookings));
        else
            return RedirectToAction(nameof(MyBookings));
    }




}
}