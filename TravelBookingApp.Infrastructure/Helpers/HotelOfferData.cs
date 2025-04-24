using System.Text.Json.Serialization;

namespace TravelBookingApp.Infrastructure.Helpers
{
    public class HotelOfferData
    {
        [JsonPropertyName("hotel")]
        public HotelData Hotel { get; set; }

        [JsonPropertyName("offers")]
        public List<HotelOfferDetails> Offers { get; set; }
    }
}
