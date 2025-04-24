using Microsoft.Extensions.Logging;
using TravelBookingApp.Core.Interfaces;
using TravelBookingApp.Domain.Models;

namespace TravelBookingApp.Core.Services
{
    public class FlightService : IFlightService
    {
        private readonly IFlightRepository _flightRepository;
        private readonly IBookingRepository _bookingRepository;
        private readonly IExternalTravelApiService _externalApiService;
        private readonly ILogger<FlightService> _logger;

        public FlightService(
            IFlightRepository flightRepository,
            IBookingRepository bookingRepository,
            IExternalTravelApiService externalApiService,
            ILogger<FlightService> logger)
        {
            _flightRepository = flightRepository;
            _bookingRepository = bookingRepository;
            _externalApiService = externalApiService;
            _logger = logger;
        }

        public async Task<IEnumerable<Flight>> SearchFlightsAsync(string departureCity, string arrivalCity, DateTime date)
        {
            try
            {
                // First check our database
                var localFlights = await _flightRepository.SearchFlightsAsync(departureCity, arrivalCity, date);

                // Then check external API
                var externalFlights = await _externalApiService.SearchFlightsAsync(departureCity, arrivalCity, date);

                // Combine results
                return localFlights.Concat(externalFlights);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error searching for flights");
                throw new ApplicationException("Failed to search flights", ex);
            }
        }

        public async Task<Flight> GetFlightByIdAsync(Guid id)
        {
            var flight = await _flightRepository.GetByIdAsync(id);
            if (flight == null)
            {
                throw new KeyNotFoundException($"Flight with ID {id} not found");
            }
            return flight;
        }

        public async Task<Booking> BookFlightAsync(Guid flightId, Guid userId, DateTime bookingDate)
        {
            var flight = await GetFlightByIdAsync(flightId);

            if (flight.AvailableSeats <= 0)
            {
                throw new InvalidOperationException("No available seats on this flight");
            }

            var booking = new Booking
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                Type = BookingType.Flight,
                ItemId = flightId,
                BookingDate = DateTime.UtcNow,
                StartDate = flight.DepartureTime,
                EndDate = flight.ArrivalTime,
                TotalPrice = flight.Price,
                Status = BookingStatus.Confirmed
            };

            await _bookingRepository.AddAsync(booking);

            // Update available seats
            flight.AvailableSeats--;
            await _flightRepository.UpdateAsync(flight);

            return booking;
        }
    }
}
