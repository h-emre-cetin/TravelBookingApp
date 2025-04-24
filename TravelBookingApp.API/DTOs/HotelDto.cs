namespace TravelBookingApp.API.DTOs
{
    public class HotelDto
    {
        public Guid Id { get; set; }
        
        public string Name { get; set; }
        
        public string City { get; set; }
        
        public string Address { get; set; }
        
        public int StarRating { get; set; }
        
        public decimal PricePerNight { get; set; }
        
        public int AvailableRooms { get; set; }
        
        public List<string> Amenities { get; set; }
    }
}
