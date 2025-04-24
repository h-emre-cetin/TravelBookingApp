using Microsoft.Extensions.Logging;
using TravelBookingApp.Core.Interfaces;
using TravelBookingApp.Domain.Models;

namespace TravelBookingApp.Core.Services
{
    public class HotelService : IHotelService
    {
        private readonly IHotelRepository _hotelRepository;
        private readonly IBookingRepository _bookingRepository;
        private readonly IExternalTravelApiService _externalApiService;
        private readonly ILogger<HotelService> _logger;

        public HotelService(
            IHotelRepository hotelRepository,
            IBookingRepository bookingRepository,
            IExternalTravelApiService externalApiService,
            ILogger<HotelService> logger)
        {
            _hotelRepository = hotelRepository;
            _bookingRepository = bookingRepository;
            _externalApiService = externalApiService;
            _logger = logger;
        }

        public async Task<IEnumerable<Hotel>> SearchHotelsAsync(string city, DateTime checkIn, DateTime checkOut)
        {
            try
            {
                // First check our database
                var localHotels = await _hotelRepository.SearchHotelsAsync(city, checkIn, checkOut);

                // Then check external API
                var externalHotels = await _externalApiService.SearchHotelsAsync(city, checkIn, checkOut);

                // Combine results
                return localHotels.Concat(externalHotels);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error searching for hotels");
                throw new ApplicationException("Failed to search hotels", ex);
            }
        }

        public async Task<Hotel> GetHotelByIdAsync(Guid id)
        {
            var hotel = await _hotelRepository.GetByIdAsync(id);
            if (hotel == null)
            {
                throw new KeyNotFoundException($"Hotel with ID {id} not found");
            }
            return hotel;
        }

        public async Task<Booking> BookHotelAsync(Guid hotelId, Guid userId, DateTime checkIn, DateTime checkOut)
        {
            var hotel = await GetHotelByIdAsync(hotelId);

            if (hotel.AvailableRooms <= 0)
            {
                throw new InvalidOperationException("No available rooms in this hotel");
            }

            // Calculate number of nights
            int nights = (int)(checkOut - checkIn).TotalDays;
            if (nights <= 0)
            {
                throw new ArgumentException("Check-out date must be after check-in date");
            }

            var booking = new Booking
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                Type = BookingType.Hotel,
                ItemId = hotelId,
                BookingDate = DateTime.UtcNow,
                StartDate = checkIn,
                EndDate = checkOut,
                TotalPrice = hotel.PricePerNight * nights,
                Status = BookingStatus.Confirmed
            };

            await _bookingRepository.AddAsync(booking);

            // Update available rooms
            hotel.AvailableRooms--;
            await _hotelRepository.UpdateAsync(hotel);

            return booking;
        }
    }

}
