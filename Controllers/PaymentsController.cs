using EventBooking.Data;
using EventBooking.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;

[Authorize]
public class PaymentsController : Controller
{
    private readonly ApplicationDbContext _context;

    public PaymentsController(ApplicationDbContext context)
    {
        _context = context;
    }

    // STEP 1: Show payment form
    [HttpGet]
    public async Task<IActionResult> Pay(int bookingId)
    {
        var booking = await _context.PackageBookings
            .Include(b => b.Package)
            .FirstOrDefaultAsync(b => b.PackageBookingId == bookingId);

        if (booking == null || booking.Status != BookingStatuss.Approved)
            return NotFound();

        var model = new Payments
        {
            PackageBookingId = booking.PackageBookingId,
            Amount = booking.Package.TotalPrice,
            Booking = booking
        };

        return View(model);
    }

    // STEP 2: Handle payment submission
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Pay(Payments model)
    {
        var booking = await _context.PackageBookings
            .Include(b => b.Package)
            .FirstOrDefaultAsync(b => b.PackageBookingId == model.PackageBookingId);

        if (booking == null) return NotFound();

        // Always fetch price from DB for safety
        model.Amount = booking.Package.TotalPrice;

        // Fake payment validation (replace with Stripe/PayFast later)
        if (model.CardNumber.Length == 16 && model.CVV.Length == 3)
        {
            model.IsSuccessful = true;
        }

        _context.Payment.Add(model);

        // If successful, mark booking as Paid and send email
        if (model.IsSuccessful)
        {
            booking.Status = BookingStatuss.Paid;
            _context.PackageBookings.Update(booking);

            // Send confirmation email to ContactEmail
            await SendPaymentConfirmationEmail(booking);
        }

        await _context.SaveChangesAsync();

        TempData["Success"] = model.IsSuccessful
            ? $"Payment successful for {booking.Package.PackageName}, amount: {booking.Package.TotalPrice:C}"
            : "Payment failed. Please check your card details.";

        return RedirectToAction("MyBookings", "Package");
    }

    // ------------------ SMTP Email ------------------
    private async Task SendPaymentConfirmationEmail(PackageBooking booking)
    {
        if (string.IsNullOrEmpty(booking.ContactEmail))
            return;

        try
        {
            using (var smtp = new SmtpClient("smtp.gmail.com", 587))
            {
                smtp.Credentials = new NetworkCredential("kwazi9939@gmail.com", "gido oabp fcgg idwc"); // Use app password
                smtp.EnableSsl = true;

                var mail = new MailMessage
                {
                    From = new MailAddress("kwazi9939@gmail.com", "Event Booking System"),
                    Subject = $"Payment Confirmed - {booking.Package.PackageName}",
                    IsBodyHtml = true,
                    Body = $@"
                        Hi {booking.ContactName},<br/><br/>
                        Your payment for <b>{booking.Package.PackageName}</b> has been successfully processed.<br/>
                        Amount: {booking.Package.TotalPrice:C}<br/>
                        Booking Date: {booking.BookingDate:MMM dd, yyyy}<br/><br/>
                        Thank you for booking with us!<br/>
                        Event Booking Team"
                };

                mail.To.Add(booking.ContactEmail);

                await smtp.SendMailAsync(mail);
            }
        }
        catch (System.Exception ex)
        {
            // Optional: log the error
            Console.WriteLine("SMTP Error: " + ex.Message);
        }
    }
}
