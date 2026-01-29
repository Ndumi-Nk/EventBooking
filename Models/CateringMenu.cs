
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;

namespace EventBooking.Models
{
    public class CateringMenu
    {
        [Key]
        public int MenuId { get; set; }

        [Required]
        [StringLength(100)]
        public string MenuName { get; set; } = string.Empty;

        [Required]
        [StringLength(500)]
        public string Description { get; set; } = string.Empty;

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal PricePerPerson { get; set; }

        [Required]
        public MenuType MenuType { get; set; }

        [StringLength(1000)]
        public string? IncludedItems { get; set; }

        public bool IsVegetarian { get; set; }
        public bool IsVegan { get; set; }
        public bool HasGlutenFree { get; set; }
        public bool IsActive { get; set; } = true;
        public string? ImagePath { get; set; }
    }

    public enum MenuType
        {
            Standard,
            Premium,
            Deluxe,
            Vegan,
            Vegetarian,
            Kids,
            Corporate,
            Funeral,
            Wedding
        }

        public class BookingCatering
        {
            [Key]
            public int BookingCateringId { get; set; }

            [Required]
            public int BookingId { get; set; }

            [Required]
            public int MenuId { get; set; }

            [Required]
            [Range(1, 1000)]
            public int Quantity { get; set; }

            [Column(TypeName = "decimal(18,2)")]
            public decimal UnitPrice { get; set; }

            [Column(TypeName = "decimal(18,2)")]
            public decimal TotalPrice { get; set; }

            [StringLength(500)]
            public string SpecialInstructions { get; set; }

            [ForeignKey("BookingId")]
            public virtual Booking Booking { get; set; }

            [ForeignKey("MenuId")]
            public virtual CateringMenu CateringMenu { get; set; }
        }
    }