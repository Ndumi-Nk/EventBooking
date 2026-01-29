
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;

namespace EventBooking.Models
{
    public class Booking
    {
        [Key]
        public int BookingId { get; set; }

        [Required]
        public string UserId { get; set; } = string.Empty;

        [Required]
        public int EventId { get; set; }

        [Required]
        public EventType Type { get; set; }

        [Required]
        [StringLength(500)]
        public string SpecialRequirements { get; set; } = string.Empty;

        [Required]
        [Range(1, 1000)]
        public int NumberOfPeople { get; set; }

        // Additional services flags
        public bool RequiresCatering { get; set; }
        public bool RequiresAudioVisual { get; set; }
        public bool RequiresDecorations { get; set; }
        public bool RequiresSecurity { get; set; }
        public bool RequiresParking { get; set; }

        // Financials
        [Column(TypeName = "decimal(18,2)")]
        public decimal BaseAmount { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal CateringCost { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal AdditionalServicesCost { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal TotalAmount { get; set; }

        // Status and tracking
        public BookingStatus Status { get; set; } = BookingStatus.Pending;

        [StringLength(1000)]
        public string? AdminNotes { get; set; }

        // Event timing
        public DateTime EventStartTime { get; set; }
        public DateTime EventEndTime { get; set; }

        // Contact information
        [StringLength(100)]
        public string ContactPerson { get; set; } = string.Empty;

        [StringLength(20)]
        public string ContactPhone { get; set; } = string.Empty;

        public DateTime BookingDate { get; set; } = DateTime.Now;
        public DateTime? ApprovedDate { get; set; }
        public DateTime? PaymentDate { get; set; }

        // Navigation properties
        [ForeignKey("UserId")]
        public virtual ApplicationUser User { get; set; } = null!;

        [ForeignKey("EventId")]
        public virtual Event Event { get; set; } = null!;

        public virtual ICollection<BookingCatering> BookingCaterings { get; set; } = new List<BookingCatering>();
        public virtual ICollection<BookingService> BookingServices { get; set; } = new List<BookingService>();
        public virtual ICollection<Payment> Payments { get; set; } = new List<Payment>();
    }
    public enum BookingStatus
    {
        Pending,
        Approved,
        Rejected,
        Paid,
        Completed,
        Cancelled
    }



    public class BookingService
        {
            [Key]
            public int BookingServiceId { get; set; }

            [Required]
            public int BookingId { get; set; }

            [Required]
            public int ServiceId { get; set; }

            public int Quantity { get; set; } = 1;
            public int Hours { get; set; } = 1;

            [Column(TypeName = "decimal(18,2)")]
            public decimal UnitPrice { get; set; }

            [Column(TypeName = "decimal(18,2)")]
            public decimal TotalPrice { get; set; }
        [StringLength(50)]
        public string? PriceType { get; set; }

        [ForeignKey("BookingId")]
            public virtual Booking Booking { get; set; }

            [ForeignKey("ServiceId")]
            public virtual AdditionalService AdditionalService { get; set; }
        }

        public class Payment
        {
            [Key]
            public int PaymentId { get; set; }

            [Required]
            public int BookingId { get; set; }

            [Required]
            [Column(TypeName = "decimal(18,2)")]
            public decimal Amount { get; set; }

            [Required]
            public string PaymentMethod { get; set; } // CreditCard, PayPal, BankTransfer

            [StringLength(50)]
            public string TransactionId { get; set; }

            public DateTime PaymentDate { get; set; } = DateTime.Now;
            public PaymentStatus Status { get; set; } = PaymentStatus.Pending;

            [StringLength(500)]
            public string Notes { get; set; }

            [ForeignKey("BookingId")]
            public virtual Booking Booking { get; set; }
        }

        public enum PaymentStatus
        {
            Pending,
            Completed,
            Failed,
            Refunded
        }
    }