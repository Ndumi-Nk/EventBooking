namespace EventBooking.ViewModels
{
    public class UnifiedBookingViewModel
    {
        public int BookingId { get; set; }
        public string BookingType { get; set; } // Event or Package
        public string EventOrPackageName { get; set; }
        public string UserName { get; set; }
        public int NumberOfPeople { get; set; }
        public string Status { get; set; }
        public DateTime BookingDate { get; set; }
        public decimal TotalAmount { get; set; }

        // For Event bookings
        public DateTime? EventStartTime { get; set; }
        public DateTime? EventEndTime { get; set; }
    }
}
