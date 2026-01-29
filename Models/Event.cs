
    using EventBooking.Models;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;

namespace EventBooking.Models
{
    public class Event
    {
        [Key]
        public int EventId { get; set; }

        [Required]
        [StringLength(100)]
        public string EventName { get; set; } = string.Empty;

        [Required]
        [StringLength(500)]
        public string Description { get; set; } = string.Empty;

        [Required]
        public string Venue { get; set; } = string.Empty;

        [Required]
        [Range(1, 1000)]
        public int MaxCapacity { get; set; }

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal PricePerPerson { get; set; }

        public string? ImagePath { get; set; }
        public bool IsActive { get; set; } = true;
        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }

    public class AdditionalService
    {
        [Key]
        public int ServiceId { get; set; }

        [Required]
        [StringLength(100)]
        public string ServiceName { get; set; } = string.Empty;

        [Required]
        [StringLength(500)]
        public string Description { get; set; } = string.Empty;

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal Price { get; set; }

        public string PriceType { get; set; } = "Fixed"; // "PerPerson", "PerHour", "Fixed"
        public bool IsActive { get; set; } = true;

        // 👇 New field for image
        [StringLength(255)]
        public string? ImagePath { get; set; }  // store relative path like "/images/decor/flowers.jpg"
    }


    public enum EventType
    {
        Wedding,
        Funeral,
        Birthday,
        Corporate,
        Conference,
        Concert,
        Seminar,
        Party,
        Other
    }

}