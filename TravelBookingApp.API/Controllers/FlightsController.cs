using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using TravelBookingApp.API.DTOs;
using TravelBookingApp.Core.Interfaces;

namespace TravelBookingApp.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class FlightsController : ControllerBase
    {
        private readonly IFlightService _flightService;
        private readonly ILogger<FlightsController> _logger;

        public FlightsController(IFlightService flightService, ILogger<FlightsController> logger)
        {
            _flightService = flightService;
            _logger = logger;
        }

        [HttpGet("search")]
        public async Task<ActionResult<IEnumerable<FlightDto>>> SearchFlights(
            [FromQuery] string departureCity,
            [FromQuery] string arrivalCity,
            [FromQuery] DateTime date)
        {
            try
            {
                if (string.IsNullOrEmpty(departureCity) || string.IsNullOrEmpty(arrivalCity))
                {
                    return BadRequest("Departure and arrival cities are required");
                }

                var flights = await _flightService.SearchFlightsAsync(departureCity, arrivalCity, date);
                return Ok(flights.Select(f => new FlightDto
                {
                    Id = f.Id,
                    FlightNumber = f.FlightNumber,
                    Airline = f.Airline,
                    DepartureCity = f.DepartureCity,
                    ArrivalCity = f.ArrivalCity,
                    DepartureTime = f.DepartureTime,
                    ArrivalTime = f.ArrivalTime,
                    Price = f.Price,
                    AvailableSeats = f.AvailableSeats
                }));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error searching for flights");
                return StatusCode(500, "An error occurred while searching for flights");
            }
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<FlightDto>> GetFlight(Guid id)
        {
            try
            {
                var flight = await _flightService.GetFlightByIdAsync(id);
                return Ok(new FlightDto
                {
                    Id = flight.Id,
                    FlightNumber = flight.FlightNumber,
                    Airline = flight.Airline,
                    DepartureCity = flight.DepartureCity,
                    ArrivalCity = flight.ArrivalCity,
                    DepartureTime = flight.DepartureTime,
                    ArrivalTime = flight.ArrivalTime,
                    Price = flight.Price,
                    AvailableSeats = flight.AvailableSeats
                });
            }
            catch (KeyNotFoundException)
            {
                return NotFound($"Flight with ID {id} not found");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving flight");
                return StatusCode(500, "An error occurred while retrieving the flight");
            }
        }

        [HttpPost("{id}/book")]
        public async Task<ActionResult<BookingDto>> BookFlight(Guid id, [FromBody] BookFlightRequest request)
        {
            try
            {
                var booking = await _flightService.BookFlightAsync(id, request.UserId, DateTime.UtcNow);
                return Ok(new BookingDto
                {
                    Id = booking.Id,
                    Type = booking.Type.ToString(),
                    StartDate = booking.StartDate,
                    EndDate = booking.EndDate,
                    TotalPrice = booking.TotalPrice,
                    Status = booking.Status.ToString()
                });
            }
            catch (KeyNotFoundException)
            {
                return NotFound($"Flight with ID {id} not found");
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error booking flight");
                return StatusCode(500, "An error occurred while booking the flight");
            }
        }
    }
}
