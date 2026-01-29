using EventBooking.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace EventBooking.ViewModels
{
    public class CreateBookingViewModel
    {
        public int EventId { get; set; }

        [Display(Name = "Event Type")]
        [Required(ErrorMessage = "Please select event type")]
        public EventType EventType { get; set; }

        [Display(Name = "Number of People")]
        [Required(ErrorMessage = "Please enter number of people")]
        [Range(1, 1000, ErrorMessage = "Number of people must be between 1 and 1000")]
        public int NumberOfPeople { get; set; }

        [Display(Name = "Event Start Time")]
        [Required(ErrorMessage = "Please select event start time")]
        public DateTime EventStartTime { get; set; } = DateTime.Now.AddHours(1);

        [Display(Name = "Event End Time")]
        [Required(ErrorMessage = "Please select event end time")]
        public DateTime EventEndTime { get; set; } = DateTime.Now.AddHours(4);

        [Display(Name = "Contact Person")]
        [Required(ErrorMessage = "Please enter contact person name")]
        public string ContactPerson { get; set; } = string.Empty;

        [Display(Name = "Contact Phone")]
        [Required(ErrorMessage = "Please enter contact phone number")]
        public string ContactPhone { get; set; } = string.Empty;

        [Display(Name = "Special Requirements")]
        [StringLength(500, ErrorMessage = "Special requirements cannot exceed 500 characters")]
        public string SpecialRequirements { get; set; } = string.Empty;

        // Catering Options
        [Display(Name = "Require Catering?")]
        public bool RequiresCatering { get; set; }

        public List<int> SelectedMenuIds { get; set; } = new List<int>();
        public Dictionary<int, int> MenuQuantities { get; set; } = new Dictionary<int, int>();
        public Dictionary<int, string> MenuInstructions { get; set; } = new Dictionary<int, string>();

        // Additional Services
        [Display(Name = "Audio Visual Equipment")]
        public bool RequiresAudioVisual { get; set; }

        [Display(Name = "Decorations")]
        public bool RequiresDecorations { get; set; }

        [Display(Name = "Security Services")]
        public bool RequiresSecurity { get; set; }

        [Display(Name = "Parking Arrangements")]
        public bool RequiresParking { get; set; }

        // Event details for display
        public string EventName { get; set; } = string.Empty;
        public DateTime EventDate { get; set; }
        public string Venue { get; set; } = string.Empty;
        public decimal PricePerPerson { get; set; }
        public List<CateringMenu> AvailableMenus { get; set; } = new List<CateringMenu>();
        public List<AdditionalService> AvailableServices { get; set; } = new List<AdditionalService>();
    }

    public class CreatePackageViewModel
    {
        [Required(ErrorMessage = "Please enter a package name.")]
        public string PackageName { get; set; } = string.Empty;

        public string Description { get; set; } = string.Empty;

        [Required(ErrorMessage = "Please select an event.")]
        public int EventId { get; set; }

        public IFormFile? ImageFile { get; set; }

        [Required(ErrorMessage = "Please enter the total price.")]
        [Range(1, double.MaxValue, ErrorMessage = "Total price must be greater than 0.")]
        public decimal TotalPrice { get; set; }

        // Optional: require at least one catering
        [MinLength(1, ErrorMessage = "Please select at least one catering menu.")]
        public List<int>? CateringIds { get; set; }

        // Optional: require at least one service
        [MinLength(1, ErrorMessage = "Please select at least one additional service.")]
        public List<int>? ServiceIds { get; set; }

        // Strongly-typed lists for dropdowns
        public List<Event>? AvailableEvents { get; set; }
        public List<CateringMenu>? AvailableCaterings { get; set; }
        public List<AdditionalService>? AvailableServices { get; set; }
    }

 

public class PackageViewModel
    {
        public int PackageId { get; set; }
        public string PackageName { get; set; } = string.Empty;
        public string? Description { get; set; }
        public int EventId { get; set; }
        public List<int> SelectedCateringIds { get; set; } = new();
        public List<int> SelectedServiceIds { get; set; } = new();
        public decimal TotalPrice { get; set; }
        public bool IsActive { get; set; } = true;
        public string? ImagePath { get; set; }

        // For file upload
        public IFormFile? ImageFile { get; set; }

        // Dropdowns
        public IEnumerable<Event> Events { get; set; } = new List<Event>();
        public IEnumerable<CateringMenu> CateringMenus { get; set; } = new List<CateringMenu>();
        public IEnumerable<AdditionalService> AdditionalServices { get; set; } = new List<AdditionalService>();

    }
   


    public class PackageBookingViewModel
        {
            public int PackageId { get; set; }
            public string PackageName { get; set; } = string.Empty;
            public string EventName { get; set; } = string.Empty;
            public int MaxCapacity { get; set; }

            [Required]
            [Range(1, int.MaxValue, ErrorMessage = "Number of people must be at least 1.")]
            public int NumberOfPeople { get; set; }

            [Required]
            [DataType(DataType.Date)]
            public DateTime BookingDate { get; set; }
        public decimal TotalPrice { get; set; }
        public string? ImagePath { get; set; }

        [Required]
        [StringLength(100)]
        public string ContactName { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        public string ContactEmail { get; set; } = string.Empty;

        [Required]
        [Phone]
        public string ContactPhone { get; set; } = string.Empty;

        public string? SpecialRequests { get; set; }
    }

    public class ContactViewModel
    {
        [Required(ErrorMessage = "Name is required")]
        public string Name { get; set; }

        [Required(ErrorMessage = "Email is required"), EmailAddress(ErrorMessage = "Invalid email address")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Subject is required")]
        public string Subject { get; set; }

        [Required(ErrorMessage = "Message is required")]
        public string Message { get; set; }
    }

    public class BookingDetailsViewModel
    {
        public Booking Booking { get; set; } = null!;
        public List<BookingCatering> CateringItems { get; set; } = new List<BookingCatering>();
        public List<BookingService> AdditionalServices { get; set; } = new List<BookingService>();
        public List<Payment> Payments { get; set; } = new List<Payment>();
        public string ContactPerson { get; set; }
        public string ContactPhone { get; set; }
    }
    
    public class BookingApprovalViewModel
    {
        public int BookingId { get; set; }
        public string CustomerName { get; set; } = string.Empty;
        public string EventName { get; set; } = string.Empty;
        public int NumberOfPeople { get; set; }
        public bool RequiresCatering { get; set; }
        public decimal TotalAmount { get; set; }
        public DateTime EventDate { get; set; }

        [Required(ErrorMessage = "Approval status is required")]
        public BookingStatus Status { get; set; }

        [Display(Name = "Admin Notes")]
        [StringLength(1000, ErrorMessage = "Notes cannot exceed 1000 characters")]
        public string AdminNotes { get; set; } = string.Empty;
    }
}