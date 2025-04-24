using System.Text.Json.Serialization;

namespace TravelBookingApp.Infrastructure.Helpers
{
    public class FlightOffer
    {
        [JsonPropertyName("itineraries")]
        public List<Itinerary> Itineraries { get; set; }

        
        [JsonPropertyName("price")]
        public Price Price { get; set; }
    }
}
