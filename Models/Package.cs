using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EventBooking.Models
{
    public class Package
    {
        [Key]
        public int PackageId { get; set; }

        [Required]
        [StringLength(100)]
        public string PackageName { get; set; } = string.Empty;

        [StringLength(500)]
        public string? Description { get; set; }

        [Required]
        public int EventId { get; set; }
        [ForeignKey("EventId")]
        public virtual Event Event { get; set; }

        public virtual ICollection<PackageCatering> PackageCaterings { get; set; } = new List<PackageCatering>();
        public virtual ICollection<PackageService> PackageServices { get; set; } = new List<PackageService>();

        [Column(TypeName = "decimal(18,2)")]
        public decimal TotalPrice { get; set; }

        public bool IsActive { get; set; } = true;
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        // Optional: Thumbnail for marketing
        [StringLength(255)]
        public string? ImagePath { get; set; }
    }
    public class Payments
    {
        [Key]
        public int PaymentId { get; set; }

        [Required]
        public int PackageBookingId { get; set; }

        [ForeignKey("PackageBookingId")]
        public virtual PackageBooking Booking { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal Amount { get; set; }

        [Required, StringLength(100)]
        public string CardHolderName { get; set; }

        [Required, StringLength(16, MinimumLength = 16)]
        public string CardNumber { get; set; }

        [Required, StringLength(5)] // MM/YY
        public string ExpiryDate { get; set; }
        [Required(ErrorMessage = "Please select a payment type.")]
        public string SelectedPaymentType { get; set; }
        [Required, StringLength(3, MinimumLength = 3)]
        public string CVV { get; set; }

        public DateTime PaymentDate { get; set; } = DateTime.Now;

        public bool IsSuccessful { get; set; } = false;
    }


    public class PackageCatering
    {
        [Key]
        public int PackageCateringId { get; set; }

        [Required]
        public int PackageId { get; set; }
        public virtual Package Package { get; set; }

        [Required]
        public int MenuId { get; set; }
        public virtual CateringMenu CateringMenu { get; set; }
    }

    public class PackageService
    {
        [Key]
        public int PackageServiceId { get; set; }

        [Required]
        public int PackageId { get; set; }
        public virtual Package Package { get; set; }

        [Required]
        public int ServiceId { get; set; }
        public virtual AdditionalService AdditionalService { get; set; }
    }
    public enum BookingStatuss
    {
        Pending,
        Approved,
        Cancelled,
        Paid,
        Declined
    }

    public class PackageBooking
    {
        public int PackageBookingId { get; set; }

        // Foreign Keys
        public int PackageId { get; set; }
        public string UserId { get; set; } = string.Empty;

        // Booking Details
        public int NumberOfPeople { get; set; }
        public DateTime BookingDate { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public BookingStatuss Status { get; set; } = BookingStatuss.Pending;

        // Contact Info
        public string ContactName { get; set; } = string.Empty;
        public string ContactEmail { get; set; } = string.Empty;
        public string ContactPhone { get; set; } = string.Empty;

        // Optional: special requests
        public string? SpecialRequests { get; set; }

        // Navigation Property
        public Package Package { get; set; } = null!;
    }
    public class CustomDecoration
    {
        [Key]
        public int DecorationId { get; set; }

        [Required, StringLength(100)]
        public string Name { get; set; }

        [StringLength(250)]
        public string Description { get; set; }

        [Required, Range(0, 100000)]
        public decimal Price { get; set; }

        [StringLength(250)]
        public string ImageUrl { get; set; } // Stores image path

        // Foreign key to Identity user
        [Required]
        public string UserId { get; set; }

        [ForeignKey("UserId")]
        public virtual ApplicationUser User { get; set; }  // Navigation property
    }



}
