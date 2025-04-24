using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace TravelBookingApp.Infrastructure.Helpers
{
    public class FlightOffersResponse
    {
        [JsonPropertyName("data")]
        public List<FlightOffer> Data { get; set; }
    }
}
