using EventBooking.Data;
using EventBooking.Models;
using EventBooking.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace EventBooking.Controllers
{
    [Authorize]
    public class PaymentController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public PaymentController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: Payment/Create/5
        public async Task<IActionResult> Create(int bookingId)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
                return RedirectToAction("Login", "Account");

            var booking = await _context.Bookings
                .Include(b => b.Event)
                .Include(b => b.BookingCaterings)
                    .ThenInclude(c => c.CateringMenu)
                .Include(b => b.BookingServices)
                    .ThenInclude(s => s.AdditionalService)
                .FirstOrDefaultAsync(b => b.BookingId == bookingId && b.UserId == user.Id);

            if (booking == null || booking.Status != BookingStatus.Approved)
            {
                TempData["ErrorMessage"] = "Booking not found or not approved for payment.";
                return RedirectToAction("MyBookings", "Bookings");
            }

            // Calculate total price including catering and services
            decimal cateringTotal = booking.BookingCaterings.Sum(c => c.TotalPrice);
            decimal serviceTotal = booking.BookingServices.Sum(s => s.TotalPrice);
            decimal totalAmount = booking.TotalAmount + cateringTotal + serviceTotal;

            var viewModel = new PaymentViewModel
            {
                BookingId = booking.BookingId,
                EventName = booking.Event?.EventName ?? "Unknown Event",
                CustomerName = $"{user.FirstName} {user.LastName}",
                Amount = totalAmount,
                CateringItems = booking.BookingCaterings.Select(c => new PaymentItemViewModel
                {
                    Name = c.CateringMenu.MenuName,
                    Quantity = c.Quantity,
                    UnitPrice = c.UnitPrice,
                    TotalPrice = c.TotalPrice
                }).ToList(),
                ServiceItems = booking.BookingServices.Select(s => new PaymentItemViewModel
                {
                    Name = s.AdditionalService?.ServiceName ?? "Service",
                    Quantity = s.Quantity,
                    UnitPrice = s.UnitPrice,
                    TotalPrice = s.TotalPrice
                }).ToList()
            };

            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(PaymentViewModel viewModel)
        {
            if (!ModelState.IsValid)
                return View(viewModel);

            var user = await _userManager.GetUserAsync(User);
            if (user == null)
                return RedirectToAction("Login", "Account");

            var booking = await _context.Bookings
                .Include(b => b.Event)
                .Include(b => b.BookingCaterings)
                    .ThenInclude(c => c.CateringMenu)
                .Include(b => b.BookingServices)
                    .ThenInclude(s => s.AdditionalService)
                .FirstOrDefaultAsync(b => b.BookingId == viewModel.BookingId && b.UserId == user.Id);

            if (booking == null || booking.Status != BookingStatus.Approved)
            {
                TempData["ErrorMessage"] = "Invalid booking for payment.";
                return RedirectToAction("MyBookings", "Bookings");
            }

            var payment = new Payment
            {
                BookingId = booking.BookingId,
                Amount = viewModel.Amount,
                PaymentMethod = viewModel.PaymentMethod,
                TransactionId = GenerateTransactionId(),
                Status = PaymentStatus.Completed, // simulate success
                Notes = $"Payment processed via {viewModel.PaymentMethod}"
            };

            _context.Payments.Add(payment);

            booking.Status = BookingStatus.Paid;
            booking.PaymentDate = DateTime.Now;

            _context.Bookings.Update(booking);
            await _context.SaveChangesAsync();

            // Send payment confirmation email
            try
            {
                using (var smtp = new System.Net.Mail.SmtpClient("smtp.gmail.com", 587))
                {
                    smtp.Credentials = new System.Net.NetworkCredential("kwazi9939@gmail.com", "gido oabp fcgg idwc");
                    smtp.EnableSsl = true;

                    var mail = new System.Net.Mail.MailMessage
                    {
                        From = new System.Net.Mail.MailAddress("kwazi9939@gmail.com", "Event Booking System"),
                        Subject = $"Payment Confirmation - {booking.Event?.EventName}",
                        IsBodyHtml = true,
                        Body = $@"
                    Dear {user.FirstName} {user.LastName},<br/><br/>
                    Your payment for <b>{booking.Event?.EventName}</b> has been successfully processed.<br/>
                    Amount Paid: {payment.Amount:C}<br/>
                    Payment Method: {payment.PaymentMethod}<br/>
                    Transaction ID: {payment.TransactionId}<br/><br/>
                    Thank you for booking with us!
                "
                    };

                    mail.To.Add(user.Email);
                    await smtp.SendMailAsync(mail);
                }
            }
            catch (Exception ex)
            {
                // log exception if needed
                Console.WriteLine("SMTP Error: " + ex.Message);
            }

            TempData["SuccessMessage"] = "Payment processed successfully! Your booking is now confirmed.";
            return RedirectToAction("Receipt", "Payment", new { paymentId = payment.PaymentId });
        }

        private string GenerateTransactionId()
        {
            return "TXN" + System.DateTime.Now.ToString("yyyyMMddHHmmss") + new System.Random().Next(1000, 9999);
        }
        public async Task<IActionResult> Receipt(int paymentId)
        {
            // Load payment with all related data including Booking, Event, User, Caterings, and Services
            var payment = await _context.Payments
                .Include(p => p.Booking)
                    .ThenInclude(b => b.Event)
                .Include(p => p.Booking)
                    .ThenInclude(b => b.User)
                .Include(p => p.Booking.BookingCaterings)
                    .ThenInclude(c => c.CateringMenu)
                .Include(p => p.Booking.BookingServices)
                    .ThenInclude(s => s.AdditionalService)
                .FirstOrDefaultAsync(p => p.PaymentId == paymentId);

            if (payment == null)
                return NotFound();

            var booking = payment.Booking;

            // Null-safe handling
            var viewModel = new PaymentViewModel
            {
                BookingId = booking?.BookingId ?? 0,
                EventName = booking?.Event?.EventName ?? "Unknown Event",
                CustomerName = booking?.User != null
                    ? $"{booking.User.FirstName} {booking.User.LastName}"
                    : "Unknown Customer",
                Amount = payment.Amount,
                PaymentMethod = payment.PaymentMethod ?? "Unknown",
                CateringItems = booking?.BookingCaterings?.Select(c => new PaymentItemViewModel
                {
                    Name = c.CateringMenu?.MenuName ?? "Unknown Menu",
                    Quantity = c.Quantity,
                    UnitPrice = c.UnitPrice,
                    TotalPrice = c.TotalPrice
                }).ToList() ?? new List<PaymentItemViewModel>(),
                ServiceItems = booking?.BookingServices?.Select(s => new PaymentItemViewModel
                {
                    Name = s.AdditionalService?.ServiceName ?? "Service",
                    Quantity = s.Quantity,
                    UnitPrice = s.UnitPrice,
                    TotalPrice = s.TotalPrice
                }).ToList() ?? new List<PaymentItemViewModel>()
            };

            return View(viewModel);
        }

    }
}
