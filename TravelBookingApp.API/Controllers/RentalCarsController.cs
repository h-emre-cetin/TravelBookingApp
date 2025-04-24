using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using TravelBookingApp.API.DTOs;
using TravelBookingApp.Core.Interfaces;

namespace TravelBookingApp.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RentalCarsController : ControllerBase
    {
        private readonly IRentalCarService _carService;
        private readonly ILogger<RentalCarsController> _logger;

        public RentalCarsController(IRentalCarService carService, ILogger<RentalCarsController> logger)
        {
            _carService = carService;
            _logger = logger;
        }

        [HttpGet("search")]
        public async Task<ActionResult<IEnumerable<RentalCarDto>>> SearchCars(
            [FromQuery] string location,
            [FromQuery] DateTime pickupDate,
            [FromQuery] DateTime returnDate)
        {
            try
            {
                if (string.IsNullOrEmpty(location))
                {
                    return BadRequest("Location is required");
                }

                if (pickupDate >= returnDate)
                {
                    return BadRequest("Return date must be after pickup date");
                }

                var cars = await _carService.SearchCarsAsync(location, pickupDate, returnDate);
                return Ok(cars.Select(c => new RentalCarDto
                {
                    Id = c.Id,
                    Make = c.Make,
                    Model = c.Model,
                    Type = c.Type,
                    Year = c.Year,
                    PricePerDay = c.PricePerDay,
                    IsAvailable = c.IsAvailable,
                    Location = c.Location
                }));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error searching for rental cars");
                return StatusCode(500, "An error occurred while searching for rental cars");
            }
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<RentalCarDto>> GetCar(Guid id)
        {
            try
            {
                var car = await _carService.GetCarByIdAsync(id);
                return Ok(new RentalCarDto
                {
                    Id = car.Id,
                    Make = car.Make,
                    Model = car.Model,
                    Type = car.Type,
                    Year = car.Year,
                    PricePerDay = car.PricePerDay,
                    IsAvailable = car.IsAvailable,
                    Location = car.Location
                });
            }
            catch (KeyNotFoundException)
            {
                return NotFound($"Rental car with ID {id} not found");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving rental car");
                return StatusCode(500, "An error occurred while retrieving the rental car");
            }
        }

        [HttpPost("{id}/book")]
        public async Task<ActionResult<BookingDto>> BookCar(Guid id, [FromBody] BookCarRequest request)
        {
            try
            {
                if (request.PickupDate >= request.ReturnDate)
                {
                    return BadRequest("Return date must be after pickup date");
                }

                var booking = await _carService.BookCarAsync(id, request.UserId, request.PickupDate, request.ReturnDate);
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
                return NotFound($"Rental car with ID {id} not found");
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error booking rental car");
                return StatusCode(500, "An error occurred while booking the rental car");
            }
        }
    }
}
