using System.Text.Json.Serialization;

namespace TravelBookingApp.Infrastructure.Helpers
{
    public class HotelOfferDetails
    {
        [JsonPropertyName("id")]
        public string Id { get; set; }

        [JsonPropertyName("checkInDate")]
        public string CheckInDate { get; set; }

        [JsonPropertyName("checkOutDate")]
        public string CheckOutDate { get; set; }

        [JsonPropertyName("price")]
        public HotelPrice Price { get; set; }
    }

}
