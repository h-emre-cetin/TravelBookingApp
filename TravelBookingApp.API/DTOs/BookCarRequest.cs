namespace TravelBookingApp.API.DTOs
{
    public class BookCarRequest
    {
        public Guid UserId { get; set; }
        
        public DateTime PickupDate { get; set; }
        
        public DateTime ReturnDate { get; set; }
    }
}
