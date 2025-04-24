namespace TravelBookingApp.API.DTOs
{
    public class FlightDto
    {
        public Guid Id { get; set; }
        
        public string FlightNumber { get; set; }
        
        public string Airline { get; set; }
        
        public string DepartureCity { get; set; }
        
        public string ArrivalCity { get; set; }
        
        public DateTime DepartureTime { get; set; }
        
        public DateTime ArrivalTime { get; set; }
        
        public decimal Price { get; set; }
        
        public int AvailableSeats { get; set; }
    }
}
