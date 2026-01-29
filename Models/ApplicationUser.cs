using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace EventBooking.Models
{
    public class ApplicationUser : IdentityUser
    {
        [StringLength(100)]
        public string FirstName { get; set; }

        [StringLength(100)]
        public string LastName { get; set; }

        [StringLength(200)]
        public string Address { get; set; }
        [Phone]
        [StringLength(20)]
        public new string PhoneNumber { get; set; } // overrides IdentityUser.PhoneNumber public string PhoneNumber{get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public bool IsActive { get; set; } = true;

        [MaxLength(100)]
        public string? GoogleId { get; set; } // optional
        public virtual ICollection<CustomDecoration> CustomDecorations { get; set; } = new List<CustomDecoration>();
    }
}
