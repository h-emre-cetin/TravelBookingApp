using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace TravelBookingApp.Infrastructure.Helpers
{
    public class Itinerary
    {
        [JsonPropertyName("segments")]
        public List<Segment> Segments { get; set; }
    }
}
