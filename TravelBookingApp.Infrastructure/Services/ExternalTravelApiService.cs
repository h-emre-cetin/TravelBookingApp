using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Net.Http.Headers;
using System.Text.Json;
using TravelBookingApp.Core.Interfaces;
using TravelBookingApp.Domain.Models;
using TravelBookingApp.Infrastructure.Helpers;

namespace TravelBookingApp.Infrastructure.Services
{
    public class ExternalTravelApiService : IExternalTravelApiService
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<ExternalTravelApiService> _logger;
        private readonly string _apiKey;
        private readonly string _apiSecret;

        public ExternalTravelApiService(HttpClient httpClient, ILogger<ExternalTravelApiService> logger, IConfiguration configuration)
        {
            _httpClient = httpClient;
            _logger = logger;
            _apiKey = configuration["ExternalApi:ApiKey"];
            _apiSecret = configuration["ExternalApi:ApiSecret"];

            // We'll use Amadeus API (they have a free tier)
            _httpClient.BaseAddress = new Uri("https://test.api.amadeus.com/v2/");
        }

        private async Task<string> GetAccessTokenAsync()
        {
            // For Amadeus API, we need to get an access token first
            var tokenRequest = new HttpRequestMessage(HttpMethod.Post, "https://test.api.amadeus.com/v1/security/oauth2/token");
            tokenRequest.Content = new FormUrlEncodedContent(new Dictionary<string, string>
            {
                ["grant_type"] = "client_credentials",
                ["client_id"] = _apiKey,
                ["client_secret"] = _apiSecret
            });

            var response = await _httpClient.SendAsync(tokenRequest);
            response.EnsureSuccessStatusCode();

            var content = await response.Content.ReadAsStringAsync();
            var tokenResponse = JsonSerializer.Deserialize<TokenResponse>(content);
            return tokenResponse.AccessToken;
        }

        public async Task<IEnumerable<Flight>> SearchFlightsAsync(string departureCity, string arrivalCity, DateTime date)
        {
            try
            {
                var token = await GetAccessTokenAsync();

                var request = new HttpRequestMessage(HttpMethod.Get,
                    $"shopping/flight-offers?originLocationCode={departureCity}&destinationLocationCode={arrivalCity}&departureDate={date:yyyy-MM-dd}&adults=1");
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

                var response = await _httpClient.SendAsync(request);
                response.EnsureSuccessStatusCode();

                var content = await response.Content.ReadAsStringAsync();
                var flightOffers = JsonSerializer.Deserialize<FlightOffersResponse>(content);

                // Map the external API response to our domain model
                return MapToFlights(flightOffers);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching flights from external API");
                return Enumerable.Empty<Flight>();
            }
        }

        private IEnumerable<Flight> MapToFlights(FlightOffersResponse response)
        {
            // Implementation of mapping from API response to our domain model
            var flights = new List<Flight>();

            if (response?.Data == null) return flights;

            foreach (var offer in response.Data)
            {
                foreach (var itinerary in offer.Itineraries)
                {
                    var segment = itinerary.Segments.First();

                    flights.Add(new Flight
                    {
                        Id = Guid.NewGuid(),
                        FlightNumber = segment.Number,
                        Airline = segment.CarrierCode,
                        DepartureCity = segment.Departure.IataCode,
                        ArrivalCity = segment.Arrival.IataCode,
                        DepartureTime = DateTime.Parse(segment.Departure.At),
                        ArrivalTime = DateTime.Parse(segment.Arrival.At),
                        Price = decimal.Parse(offer.Price.Total),
                        AvailableSeats = 10 // Assuming default availability
                    });
                }
            }

            return flights;
        }

        // Similar implementations for hotel and car search
        public async Task<IEnumerable<Hotel>> SearchHotelsAsync(string city, DateTime checkIn, DateTime checkOut)
        {
            try
            {
                var token = await GetAccessTokenAsync();

                // Format dates as required by Amadeus API
                string checkInStr = checkIn.ToString("yyyy-MM-dd");
                string checkOutStr = checkOut.ToString("yyyy-MM-dd");

                // Get city code (in a real application, you might need to look up the city code first)
                string cityCode = await GetCityCodeAsync(city);
                
                // https://test.api.amadeus.com/v1/reference-data/locations/hotels/by-city?cityCode=BLR

                var request = new HttpRequestMessage(HttpMethod.Get,
                    $"reference-data/locations/hotels/by-city?cityCode{cityCode}");
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

                var response = await _httpClient.SendAsync(request);
                response.EnsureSuccessStatusCode();

                var content = await response.Content.ReadAsStringAsync();
                var hotelOffers = JsonSerializer.Deserialize<HotelOffersResponse>(content);

                // Map the external API response to our domain model
                return MapToHotels(hotelOffers, checkIn, checkOut);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching hotels from external API");
                return new List<Hotel>();
            }
        }

        private async Task<string> GetCityCodeAsync(string cityName)
        {
            try
            {
                var token = await GetAccessTokenAsync();

                var request = new HttpRequestMessage(HttpMethod.Get,
                    $"reference-data/locations?keyword={cityName}&subType=CITY");
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

                var response = await _httpClient.SendAsync(request);
                response.EnsureSuccessStatusCode();

                var content = await response.Content.ReadAsStringAsync();
                var locationResponse = JsonSerializer.Deserialize<LocationResponse>(content);

                if (locationResponse?.Data != null && locationResponse.Data.Count > 0)
                {
                    return locationResponse.Data[0].IataCode;
                }

                // Default to a common city code if not found
                return "NYC";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching city code");
                // Default to a common city code
                return "NYC";
            }
        }

        private IEnumerable<Hotel> MapToHotels(HotelOffersResponse response, DateTime checkIn, DateTime checkOut)
        {
            var hotels = new List<Hotel>();

            if (response?.Data == null) return hotels;

            foreach (var offer in response.Data)
            {
                var hotel = offer.Hotel;
                var hotelOffer = offer.Offers.FirstOrDefault();

                if (hotel != null && hotelOffer != null)
                {
                    var amenities = new List<string>();
                    if (hotel.Amenities != null)
                    {
                        amenities.AddRange(hotel.Amenities);
                    }

                    hotels.Add(new Hotel
                    {
                        Id = Guid.NewGuid(),
                        Name = hotel.Name,
                        City = hotel.CityCode,
                        Address = $"{hotel.Address?.Lines?.FirstOrDefault() ?? ""}, {hotel.Address?.CityName ?? ""}, {hotel.Address?.CountryCode ?? ""}",
                        StarRating = hotel.Rating != null ? int.Parse(hotel.Rating) : 3,
                        PricePerNight = decimal.Parse(hotelOffer.Price?.Total ?? "100"),
                        AvailableRooms = 5, // Assuming default availability
                        Amenities = amenities
                    });
                }
            }

            return hotels;
        }

        public async Task<IEnumerable<RentalCar>> SearchCarsAsync(string location, DateTime pickupDate, DateTime returnDate)
        {
            try
            {
                // Note: Amadeus API doesn't have a direct car rental endpoint in their free tier
                // This is a simulated response based on the location

                // In a real application, you would integrate with a car rental API
                // For now, we'll generate some sample data
                return GenerateSampleCars(location, 10);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching rental cars");
                return new List<RentalCar>();
            }
        }

        private IEnumerable<RentalCar> GenerateSampleCars(string location, int count)
        {
            var cars = new List<RentalCar>();
            var random = new Random();

            string[] makes = { "Toyota", "Honda", "Ford", "Chevrolet", "Nissan", "BMW", "Mercedes", "Audi" };
            string[] models = { "Corolla", "Civic", "Focus", "Cruze", "Sentra", "3 Series", "C-Class", "A4" };
            string[] types = { "Economy", "Compact", "Mid-size", "Full-size", "SUV", "Luxury" };

            for (int i = 0; i < count; i++)
            {
                int makeIndex = random.Next(makes.Length);
                int modelIndex = random.Next(models.Length);
                int typeIndex = random.Next(types.Length);

                cars.Add(new RentalCar
                {
                    Id = Guid.NewGuid(),
                    Make = makes[makeIndex],
                    Model = models[modelIndex],
                    Type = types[typeIndex],
                    Year = random.Next(2018, 2024),
                    PricePerDay = (decimal)(30 + random.Next(10, 100)),
                    IsAvailable = true,
                    Location = location
                });
            }

            return cars;
        }
    }
}
