using System.ComponentModel.DataAnnotations;

namespace EventBooking.ViewModels
{
    public class PaymentViewModel
    {
        public int BookingId { get; set; }
        public decimal Amount { get; set; }
        public string EventName { get; set; }
        public string CustomerName { get; set; }

        [Required(ErrorMessage = "Please select a payment method")]
        public string PaymentMethod { get; set; }

        [Display(Name = "Card Number")]
        [CreditCard(ErrorMessage = "Invalid credit card number")]
        public string CardNumber { get; set; }

        [Display(Name = "Expiry Date (MM/YY)")]
        [RegularExpression(@"^(0[1-9]|1[0-2])\/?([0-9]{2})$", ErrorMessage = "Invalid expiry date")]
        public string ExpiryDate { get; set; }

        [Display(Name = "CVV")]
        [RegularExpression(@"^[0-9]{3,4}$", ErrorMessage = "Invalid CVV")]
        public string CVV { get; set; }

        [Display(Name = "Name on Card")]
        public string CardHolderName { get; set; }

        // Added to fetch details for display
        public List<PaymentItemViewModel> CateringItems { get; set; } = new();
        public List<PaymentItemViewModel> ServiceItems { get; set; } = new();
    }

    public class PaymentItemViewModel
    {
        public string Name { get; set; }
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal TotalPrice { get; set; }
    }
}