namespace TravelBookingApp.API.DTOs
{
    public class BookHotelRequest
    {
        public Guid UserId { get; set; }
      
        public DateTime CheckIn { get; set; }
        
        public DateTime CheckOut { get; set; }
    }
}
