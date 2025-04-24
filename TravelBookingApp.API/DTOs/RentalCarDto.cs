namespace TravelBookingApp.API.DTOs
{
    public class RentalCarDto
    {
        public Guid Id { get; set; }
        
        public string Make { get; set; }
        
        public string Model { get; set; }
        
        public string Type { get; set; }
        
        public int Year { get; set; }
        
        public decimal PricePerDay { get; set; }
        
        public bool IsAvailable { get; set; }
        
        public string Location { get; set; }
    }
}
