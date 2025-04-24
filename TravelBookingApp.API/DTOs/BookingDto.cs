namespace TravelBookingApp.API.DTOs
{
    public class BookingDto
    {
        public Guid Id { get; set; }
        
        public string Type { get; set; }
        
        public DateTime StartDate { get; set; }
        
        public DateTime? EndDate { get; set; }
        
        public decimal TotalPrice { get; set; }
        
        public string Status { get; set; }
    }
}
