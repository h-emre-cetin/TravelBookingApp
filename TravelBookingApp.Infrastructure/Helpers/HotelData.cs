using System.Text.Json.Serialization;

namespace TravelBookingApp.Infrastructure.Helpers
{
    public class HotelData
    {
        [JsonPropertyName("hotelId")]
        public string HotelId { get; set; }

        [JsonPropertyName("name")]
        public string Name { get; set; }

        [JsonPropertyName("cityCode")]
        public string CityCode { get; set; }

        [JsonPropertyName("rating")]
        public string Rating { get; set; }

        [JsonPropertyName("address")]
        public HotelAddress Address { get; set; }

        [JsonPropertyName("amenities")]
        public List<string> Amenities { get; set; }
    }
}
