namespace TravelBookingApp.Domain.Models
{
    public class Hotel
    {
        public Guid Id { get; set; }
        
        public string Name { get; set; } = string.Empty;
        
        public string City { get; set; } = string.Empty;
        
        public string Address { get; set; } = string.Empty;
        
        public int StarRating { get; set; }
        
        public decimal PricePerNight { get; set; }
        
        public int AvailableRooms { get; set; }
        
        public List<string> Amenities { get; set; } = new List<string>();
    }
}
