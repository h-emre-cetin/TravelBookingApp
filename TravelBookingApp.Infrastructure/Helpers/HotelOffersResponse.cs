using System.Text.Json.Serialization;

namespace TravelBookingApp.Infrastructure.Helpers
{
    public class HotelOffersResponse
    {
        [JsonPropertyName("data")]
        public List<HotelOfferData> Data { get; set; }
    }
}
