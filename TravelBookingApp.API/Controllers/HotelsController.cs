using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using TravelBookingApp.API.DTOs;
using TravelBookingApp.Core.Interfaces;

namespace TravelBookingApp.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class HotelsController : ControllerBase
    {
        private readonly IHotelService _hotelService;
        private readonly ILogger<HotelsController> _logger;

        public HotelsController(IHotelService hotelService, ILogger<HotelsController> logger)
        {
            _hotelService = hotelService;
            _logger = logger;
        }

        [HttpGet("search")]
        public async Task<ActionResult<IEnumerable<HotelDto>>> SearchHotels(
            [FromQuery] string city,
            [FromQuery] DateTime checkIn,
            [FromQuery] DateTime checkOut)
        {
            try
            {
                if (string.IsNullOrEmpty(city))
                {
                    return BadRequest("City is required");
                }

                if (checkIn >= checkOut)
                {
                    return BadRequest("Check-out date must be after check-in date");
                }

                var hotels = await _hotelService.SearchHotelsAsync(city, checkIn, checkOut);
                return Ok(hotels.Select(h => new HotelDto
                {
                    Id = h.Id,
                    Name = h.Name,
                    City = h.City,
                    Address = h.Address,
                    StarRating = h.StarRating,
                    PricePerNight = h.PricePerNight,
                    AvailableRooms = h.AvailableRooms,
                    Amenities = h.Amenities
                }));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error searching for hotels");
                return StatusCode(500, "An error occurred while searching for hotels");
            }
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<HotelDto>> GetHotel(Guid id)
        {
            try
            {
                var hotel = await _hotelService.GetHotelByIdAsync(id);
                return Ok(new HotelDto
                {
                    Id = hotel.Id,
                    Name = hotel.Name,
                    City = hotel.City,
                    Address = hotel.Address,
                    StarRating = hotel.StarRating,
                    PricePerNight = hotel.PricePerNight,
                    AvailableRooms = hotel.AvailableRooms,
                    Amenities = hotel.Amenities
                });
            }
            catch (KeyNotFoundException)
            {
                return NotFound($"Hotel with ID {id} not found");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving hotel");
                return StatusCode(500, "An error occurred while retrieving the hotel");
            }
        }

        [HttpPost("{id}/book")]
        public async Task<ActionResult<BookingDto>> BookHotel(Guid id, [FromBody] BookHotelRequest request)
        {
            try
            {
                if (request.CheckIn >= request.CheckOut)
                {
                    return BadRequest("Check-out date must be after check-in date");
                }

                var booking = await _hotelService.BookHotelAsync(id, request.UserId, request.CheckIn, request.CheckOut);
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
                return NotFound($"Hotel with ID {id} not found");
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error booking hotel");
                return StatusCode(500, "An error occurred while booking the hotel");
            }
        }
    }
}
