using EventBooking.Data;
using EventBooking.Models;
using EventBooking.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;

namespace EventBooking.Controllers
{
   
    public class BookingsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public BookingsController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // ---------------- Helper Methods ----------------
        private List<Booking> GetEventCart()
        {
            var cartJson = HttpContext.Session.GetString("EventCart");
            if (string.IsNullOrEmpty(cartJson)) return new List<Booking>();
            return JsonConvert.DeserializeObject<List<Booking>>(cartJson) ?? new List<Booking>();
        }

        private void SaveEventCart(List<Booking> cart)
        {
            HttpContext.Session.SetString("EventCart", JsonConvert.SerializeObject(cart));
        }
        // ---------------- Additional Services Cart ----------------
        private List<BookingService> GetServiceCart()
        {
            var cartJson = HttpContext.Session.GetString("ServiceCart");
            if (string.IsNullOrEmpty(cartJson)) return new List<BookingService>();
            return JsonConvert.DeserializeObject<List<BookingService>>(cartJson) ?? new List<BookingService>();
        }

        private void SaveServiceCart(List<BookingService> cart)
        {
            HttpContext.Session.SetString("ServiceCart", JsonConvert.SerializeObject(cart));
        }

        private void ClearServiceCart()
        {
            HttpContext.Session.Remove("ServiceCart");
        }

        private void ClearEventCart()
        {
            HttpContext.Session.Remove("EventCart");
        }

        private List<BookingCatering> GetCateringCart()
        {
            var cartJson = HttpContext.Session.GetString("CateringCart");
            if (string.IsNullOrEmpty(cartJson)) return new List<BookingCatering>();
            return JsonConvert.DeserializeObject<List<BookingCatering>>(cartJson) ?? new List<BookingCatering>();
        }

        private void SaveCateringCart(List<BookingCatering> cart)
        {
            HttpContext.Session.SetString("CateringCart", JsonConvert.SerializeObject(cart));
        }

        private void ClearCateringCart()
        {
            HttpContext.Session.Remove("CateringCart");
        }

        private void SetMessage(string message, string type = "success")
        {
            TempData["Message"] = message;
            TempData["MessageType"] = type;
        }

        private async Task<List<Booking>> GetUserBookings()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return new List<Booking>();

            var bookings = await _context.Bookings
                .Include(b => b.Event)
                .Include(b => b.BookingCaterings)
                    .ThenInclude(c => c.CateringMenu)
                .Include(b => b.BookingServices)  // Include decoration services
                    .ThenInclude(s => s.AdditionalService) // Include service details
                .Where(b => b.UserId == user.Id)
                .OrderByDescending(b => b.BookingDate)
                .ToListAsync();

            return bookings;
        }



        private async Task<List<Booking>> GetPendingBookings()
        {
            return await _context.Bookings
                .Include(b => b.Event)
                .Include(b => b.User)
                .Include(b => b.BookingCaterings)
                    .ThenInclude(c => c.CateringMenu)
                .Include(b => b.BookingServices)
                    .ThenInclude(s => s.AdditionalService)
                .Where(b => b.Status == BookingStatus.Pending)
                .OrderBy(b => b.BookingDate)
                .ToListAsync();
        }



        // ---------------- Create Event Booking ----------------
        public async Task<IActionResult> CreateEvent(int eventId)
        {
            var eventItem = await _context.Events.FindAsync(eventId);
            if (eventItem == null)
            {
                SetMessage("Event not found.", "error");
                return RedirectToAction("Index", "Home");
            }

            // Get current logged-in user
            var user = await _userManager.GetUserAsync(User);

            var viewModel = new CreateBookingViewModel
            {
                EventId = eventItem.EventId,
                EventName = eventItem.EventName,
                Venue = eventItem.Venue,
                PricePerPerson = eventItem.PricePerPerson,
                NumberOfPeople = 1,
                EventStartTime = DateTime.Now.AddHours(1),
                EventEndTime = DateTime.Now.AddHours(4),
                ContactPerson = user != null ? $"{user.FirstName} {user.LastName}".Trim() : string.Empty,
                ContactPhone = user != null && !string.IsNullOrWhiteSpace(user.PhoneNumber) ? user.PhoneNumber : "0000000000"
            };

            ViewBag.CartCount = GetEventCart().Count + GetCateringCart().Count + GetServiceCart().Count;
            return View(viewModel);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddEventToCart(CreateBookingViewModel viewModel)
        {
            if (!ModelState.IsValid)
                return Json(new { success = false, message = "Please fill all required fields." });

            var eventItem = await _context.Events.FindAsync(viewModel.EventId);
            if (eventItem == null)
                return Json(new { success = false, message = "Event not found." });

            if (viewModel.EventEndTime <= viewModel.EventStartTime)
                return Json(new { success = false, message = "End time must be after start time." });

            // Check DB overlaps
            bool overlapInDb = await _context.Bookings
                .Where(b => b.EventId == viewModel.EventId && b.Status != BookingStatus.Cancelled)
                .AnyAsync(b => viewModel.EventStartTime < b.EventEndTime && viewModel.EventEndTime > b.EventStartTime);

            if (overlapInDb)
                return Json(new { success = false, message = "Selected time overlaps with existing booking." });

            // Check session overlaps
            var cart = GetEventCart();
            bool overlapInCart = cart.Any(c => c.EventId == viewModel.EventId &&
                                               viewModel.EventStartTime < c.EventEndTime &&
                                               viewModel.EventEndTime > c.EventStartTime);

            if (overlapInCart)
                return Json(new { success = false, message = "Selected time overlaps with cart item." });

            var totalCost = viewModel.NumberOfPeople * eventItem.PricePerPerson;

            cart.Add(new Booking
            {
                EventId = eventItem.EventId,
                Event = eventItem,
                NumberOfPeople = viewModel.NumberOfPeople,
                EventStartTime = viewModel.EventStartTime,
                EventEndTime = viewModel.EventEndTime,
                BaseAmount = totalCost,
                TotalAmount = totalCost,
                Status = BookingStatus.Pending
            });

            SaveEventCart(cart);

            return Json(new { success = true, message = "Event added to cart.", cartCount = cart.Count + GetCateringCart().Count + GetServiceCart().Count });
        }
        // GET: Bookings/Catering
        public async Task<IActionResult> Catering()
        {
            // Fetch all available catering menus
            var menus = await _context.CateringMenus.ToListAsync();
            return View(menus);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult AddCateringToCart(int MenuId, int Quantity)
        {
            // Get the selected menu
            var selectedMenu = _context.CateringMenus.Find(MenuId);
            if (selectedMenu == null)
            {
                return Json(new { success = false, message = "Menu not found." });
            }

            // Create the booking item
            var bookingCatering = new BookingCatering
            {
                MenuId = selectedMenu.MenuId,
                Quantity = Quantity,
                UnitPrice = selectedMenu.PricePerPerson,
                TotalPrice = selectedMenu.PricePerPerson * Quantity,
                CateringMenu = selectedMenu
            };

            // Get the existing cart from session
            var cartJson = HttpContext.Session.GetString("CateringCart");
            var cart = string.IsNullOrEmpty(cartJson)
                ? new List<BookingCatering>()
                : JsonConvert.DeserializeObject<List<BookingCatering>>(cartJson);

            // Check if the menu already exists in cart
            var existingItem = cart.FirstOrDefault(c => c.MenuId == MenuId);
            if (existingItem != null)
            {
                existingItem.Quantity += Quantity;
                existingItem.TotalPrice = existingItem.UnitPrice * existingItem.Quantity;
            }
            else
            {
                cart.Add(bookingCatering);
            }

            // Save the updated cart back to session
            HttpContext.Session.SetString("CateringCart", JsonConvert.SerializeObject(cart));

            // Return JSON so frontend can update cart dynamically
            return Json(new
            {
                success = true,
                message = $"{selectedMenu.MenuName} added to cart.",
                cartCount = cart.Sum(c => c.Quantity)
            });
        }

        // ---------------- View Cart ----------------
        public IActionResult Cart()
        {
            var eventCart = GetEventCart();        // your event bookings from session
            var cateringCart = GetCateringCart();  // your catering items from session
            var serviceCart = GetServiceCart();    // your additional services from session

            // Load service names from DB into the serviceCart
            foreach (var s in serviceCart)
            {
                s.AdditionalService = _context.AdditionalServices
                                              .FirstOrDefault(a => a.ServiceId == s.ServiceId);
            }

            var model = Tuple.Create(eventCart, cateringCart, serviceCart);

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult RemoveServiceFromCart(int index)
        {
            var cart = GetServiceCart();
            if (index >= 0 && index < cart.Count)
            {
                cart.RemoveAt(index);
                SaveServiceCart(cart);
                return Json(new { success = true, message = "Service removed from cart." });
            }
            return Json(new { success = false, message = "Invalid service index." });
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ConfirmCart()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
                return Json(new { success = false, message = "User not authenticated." });

            var eventCart = GetEventCart();
            var cateringCart = GetCateringCart();
            var serviceCart = GetServiceCart(); // additional services

            if (!eventCart.Any())
                return Json(new { success = false, message = "You must select at least one event to confirm your booking." });

            foreach (var evtItem in eventCart)
            {
                bool overlap = await _context.Bookings
                    .Where(b => b.EventId == evtItem.EventId && b.Status != BookingStatus.Cancelled)
                    .AnyAsync(b => evtItem.EventStartTime < b.EventEndTime && evtItem.EventEndTime > b.EventStartTime);

                if (overlap)
                    return Json(new { success = false, message = $"Booking for {evtItem.Event?.Venue} overlaps another." });

                var newBooking = new Booking
                {
                    UserId = user.Id,
                    EventId = evtItem.EventId,
                    NumberOfPeople = evtItem.NumberOfPeople,
                    EventStartTime = evtItem.EventStartTime,
                    EventEndTime = evtItem.EventEndTime,
                    BaseAmount = evtItem.BaseAmount,
                    TotalAmount = evtItem.TotalAmount,
                    Status = BookingStatus.Pending,
                    BookingDate = DateTime.Now
                };

                _context.Bookings.Add(newBooking);
                await _context.SaveChangesAsync();

                // Add catering and service items (your current code)
                var cateringForBooking = cateringCart.Where(c => c.BookingId == evtItem.BookingId).ToList();
                foreach (var cartItem in cateringForBooking)
                {
                    _context.BookingCaterings.Add(new BookingCatering
                    {
                        BookingId = newBooking.BookingId,
                        MenuId = cartItem.MenuId,
                        Quantity = cartItem.Quantity,
                        UnitPrice = cartItem.UnitPrice,
                        TotalPrice = cartItem.TotalPrice,
                        SpecialInstructions = cartItem.SpecialInstructions ?? ""
                    });
                }

                var servicesForBooking = serviceCart.Where(s => s.BookingId == evtItem.BookingId).ToList();
                foreach (var serviceItem in servicesForBooking)
                {
                    _context.BookingServices.Add(new BookingService
                    {
                        BookingId = newBooking.BookingId,
                        ServiceId = serviceItem.ServiceId,
                        BookingServiceId = serviceItem.BookingServiceId,
                        Quantity = serviceItem.Quantity,
                        UnitPrice = serviceItem.UnitPrice,
                        TotalPrice = serviceItem.TotalPrice,
                        PriceType = serviceItem.PriceType
                    });
                }

                // ----------------- SMTP Email -----------------
                try
                {
                    using (var client = new System.Net.Mail.SmtpClient("smtp.gmail.com", 587)) // change host/port
                    {
                        client.Credentials = new System.Net.NetworkCredential("kwazi9939@gmail.com", "gido oabp fcgg idwc");
                        client.EnableSsl = true;

                        var mail = new System.Net.Mail.MailMessage();
                        mail.From = new System.Net.Mail.MailAddress("kwazi9939@gmail.com", "Event Booking System");
                        mail.To.Add(user.Email); // send to logged-in user
                        mail.Subject = $"Booking Confirmation - {evtItem.Event?.EventName}";
                        mail.Body = $@"
                    Dear {user.FirstName} {user.LastName},<br/><br/>
                    Your booking for <b>{evtItem.Event?.EventName}</b> has been confirmed.<br/>
                    Number of People: {evtItem.NumberOfPeople}<br/>
                    Event Start: {evtItem.EventStartTime:MMM dd, yyyy HH:mm}<br/>
                    Event End: {evtItem.EventEndTime:MMM dd, yyyy HH:mm}<br/>
                    Total Amount: {evtItem.TotalAmount:C}<br/><br/>
                    Thank you for booking with us!
                ";
                        mail.IsBodyHtml = true;

                        client.Send(mail);
                    }
                }
                catch (Exception ex)
                {
                    // Log email sending failure, but don't block booking confirmation
                    Console.WriteLine("SMTP Error: " + ex.Message);
                }
            }

            await _context.SaveChangesAsync();

            ClearEventCart();
            ClearCateringCart();
            ClearServiceCart();

            return Json(new { success = true, message = "Your booking has been confirmed successfully! Confirmation email sent." });
        }


        // ---------------- Booking Management ----------------
        public async Task<IActionResult> MyBookings()
        {
            return View(await GetUserBookings());
        }

        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> PendingApprovals()
        {
            return View(await GetPendingBookings());
        }

   

[HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ApproveBooking(int id)
    {
        var booking = await _context.Bookings
                                    .Include(b => b.User)
                                    .Include(b => b.Event)
                                    .FirstOrDefaultAsync(b => b.BookingId == id);

        if (booking == null)
        {
            SetMessage("Booking not found.", "error");
            return RedirectToAction("PendingApprovals");
        }

        booking.Status = BookingStatus.Approved;
        _context.Update(booking);
        await _context.SaveChangesAsync();

        // Send email notification
        try
        {
            using (var smtp = new SmtpClient("smtp.gmail.com", 587))
            {
                smtp.Credentials = new NetworkCredential("kwazi9939@gmail.com", "gido oabp fcgg idwc");
                smtp.EnableSsl = true;

                var mail = new MailMessage
                {
                    From = new MailAddress("kwazi9939@gmail.com", "Event Booking System"),
                    Subject = $"Booking Approved - {booking.Event?.EventName}",
                    IsBodyHtml = true,
                    Body = $@"
                    Dear {booking.User.FirstName} {booking.User.LastName},<br/><br/>
                    Your booking for <b>{booking.Event?.EventName}</b> has been <b>approved</b>.<br/>
                    Please <a href='https://eventbooking20251003061726-efewajd4h0c5atf5.southafricanorth-01.azurewebsites.net/Account/Login'>login</a> and make the payment to reserve your spot.<br/><br/>
                    Event Start: {booking.EventStartTime:MMM dd, yyyy HH:mm}<br/>
                    Event End: {booking.EventEndTime:MMM dd, yyyy HH:mm}<br/>
                    Number of People: {booking.NumberOfPeople}<br/>
                    Total Amount: {booking.TotalAmount:C}<br/><br/>
                    Thank you for booking with us!
                "
                };

                mail.To.Add(booking.User.Email);
                await smtp.SendMailAsync(mail);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine("SMTP Error (Approval): " + ex.Message);
        }

        SetMessage("Booking approved successfully.");
        return RedirectToAction("PendingApprovals");
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CancelBooking(int id)
    {
        var booking = await _context.Bookings
                                    .Include(b => b.User)
                                    .Include(b => b.Event)
                                    .FirstOrDefaultAsync(b => b.BookingId == id);

        if (booking == null)
        {
            SetMessage("Booking not found.", "error");
            return RedirectToAction("MyBookings");
        }

        var user = await _userManager.GetUserAsync(User);
        if (booking.UserId == user.Id || User.IsInRole("Admin"))
        {
            booking.Status = BookingStatus.Cancelled;
            _context.Update(booking);
            await _context.SaveChangesAsync();

            // Send cancellation email
            try
            {
                using (var smtp = new SmtpClient("smtp.gmail.com", 587))
                {
                    smtp.Credentials = new NetworkCredential("kwazi9939@gmail.com", "gido oabp fcgg idwc");
                    smtp.EnableSsl = true;

                    var mail = new MailMessage
                    {
                        From = new MailAddress("kwazi9939@gmail.com", "Event Booking System"),
                        Subject = $"Booking Cancelled - {booking.Event?.EventName}",
                        IsBodyHtml = true,
                        Body = $@"
                        Dear {booking.User.FirstName} {booking.User.LastName},<br/><br/>
                        Your booking for <b>{booking.Event?.EventName}</b> has been <b>cancelled</b>.<br/>
                        Event Start: {booking.EventStartTime:MMM dd, yyyy HH:mm}<br/>
                        Event End: {booking.EventEndTime:MMM dd, yyyy HH:mm}<br/>
                        Number of People: {booking.NumberOfPeople}<br/>
                        Total Amount: {booking.TotalAmount:C}<br/><br/>
                        If this was a mistake, please contact support.
                    "
                    };

                    mail.To.Add(booking.User.Email);
                    await smtp.SendMailAsync(mail);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("SMTP Error (Cancellation): " + ex.Message);
            }

            SetMessage("Booking cancelled successfully.");
        }
        else
        {
            SetMessage("Unauthorized to cancel booking.", "error");
        }

        return RedirectToAction("MyBookings");
    }

    public async Task<IActionResult> Details(int id)
        {
            var booking = await _context.Bookings
                .Include(b => b.Event)
                .Include(b => b.BookingCaterings)
                    .ThenInclude(c => c.CateringMenu)
                .Include(b => b.BookingServices)
                    .ThenInclude(s => s.AdditionalService)
                .FirstOrDefaultAsync(b => b.BookingId == id);

            if (booking == null)
            {
                SetMessage("Booking not found.", "error");
                return RedirectToAction("MyBookings");
            }

            // Get logged-in user info
            var user = await _userManager.GetUserAsync(User);

            var viewModel = new BookingDetailsViewModel
            {
                Booking = booking,
                CateringItems = booking.BookingCaterings.ToList(),
                AdditionalServices = booking.BookingServices.ToList(),
                Payments = await _context.Payments
                    .Where(p => p.BookingId == booking.BookingId)
                    .ToListAsync(),
                ContactPerson = user != null ? $"{user.FirstName} {user.LastName}" : "N/A",
                ContactPhone = user?.PhoneNumber ?? "N/A"
            };

            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> PayBooking(int id)
        {
            var booking = await _context.Bookings.FindAsync(id);
            if (booking == null)
            {
                SetMessage("Booking not found.", "error");
                return RedirectToAction("MyBookings");
            }

            var user = await _userManager.GetUserAsync(User);
            if (booking.UserId != user.Id)
            {
                SetMessage("Unauthorized.", "error");
                return RedirectToAction("MyBookings");
            }

            if (booking.Status != BookingStatus.Approved)
            {
                SetMessage("Only approved bookings can be paid.", "error");
                return RedirectToAction("MyBookings");
            }

            booking.Status = BookingStatus.Paid;
            _context.Update(booking);
            await _context.SaveChangesAsync();

            SetMessage("Booking paid successfully.");
            return RedirectToAction("MyBookings");
        }
        // ---------------- Remove from Carts ----------------
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult RemoveEventFromCart(int index)
        {
            var cart = GetEventCart();
            if (index >= 0 && index < cart.Count)
            {
                cart.RemoveAt(index);
                SaveEventCart(cart);
                return Json(new { success = true, message = "Event removed from cart.", cartCount = cart.Count + GetCateringCart().Count });
            }
            return Json(new { success = false, message = "Invalid event index." });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult RemoveCateringFromCart(int index)
        {
            var cart = GetCateringCart();
            if (index >= 0 && index < cart.Count)
            {
                cart.RemoveAt(index);
                SaveCateringCart(cart);
                return Json(new { success = true, message = "Catering item removed from cart.", cartCount = GetEventCart().Count + cart.Count });
            }
            return Json(new { success = false, message = "Invalid catering index." });
        }
        // GET: Bookings/Services
        public async Task<IActionResult> Services()
        {
            var services = await _context.AdditionalServices.ToListAsync();
            return View(services);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult AddServiceToCart(int serviceId, int quantity)
        {
            var service = _context.AdditionalServices.Find(serviceId);
            if (service == null)
            {
                ViewBag.Message = "Service not found.";
                ViewBag.MessageType = "error";
                return View("Services", _context.AdditionalServices.ToList());
            }

            // Create cart item
            var bookingService = new BookingService
            {
                ServiceId = service.ServiceId,
                Quantity = quantity,
                UnitPrice = service.Price,
                TotalPrice = service.Price * quantity,
                PriceType = service.PriceType  // Save the service type
            };

            // Get current cart from session
            var cart = GetServiceCart();
            cart.Add(bookingService);
            SaveServiceCart(cart);

            ViewBag.Message = $"{service.ServiceName} added to cart.";
            ViewBag.MessageType = "success";

            return View("Services", _context.AdditionalServices.ToList());
        }



    }
}
