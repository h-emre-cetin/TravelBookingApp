namespace TravelBookingApp.Domain.Models
{
    public class Booking
    {
        public Guid Id { get; set; }
       
        public Guid UserId { get; set; }
        
        public BookingType Type { get; set; }
        
        public Guid ItemId { get; set; } // Flight, Hotel or Car ID
        
        public DateTime BookingDate { get; set; }
        
        public DateTime StartDate { get; set; }
        
        public DateTime? EndDate { get; set; }
        
        public decimal TotalPrice { get; set; }
        
        public BookingStatus Status { get; set; }
    }

    public enum BookingType
    {
        Flight,
        Hotel,
        Car
    }

    public enum BookingStatus
    {
        Pending,
        Confirmed,
        Cancelled
    }
}
