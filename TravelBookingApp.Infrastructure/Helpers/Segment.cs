using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace TravelBookingApp.Infrastructure.Helpers
{
    public class Segment
    {
        [JsonPropertyName("departure")]
        public Location Departure { get; set; }

        [JsonPropertyName("arrival")]
        public Location Arrival { get; set; }

        [JsonPropertyName("carrierCode")]
        public string CarrierCode { get; set; }

        [JsonPropertyName("number")]
        public string Number { get; set; }
    }
}
