namespace TravelBookingApp.Domain.Models
{
    public class RentalCar
    {
        public Guid Id { get; set; }
        
        public string Make { get; set; } = string.Empty;
        
        public string Model { get; set; } = string.Empty;
        
        public string Type { get; set; } = string.Empty;
        
        public int Year { get; set; }
        
        public decimal PricePerDay { get; set; }
        
        public bool IsAvailable { get; set; }
        
        public string Location { get; set; } = string.Empty;
    }
}
