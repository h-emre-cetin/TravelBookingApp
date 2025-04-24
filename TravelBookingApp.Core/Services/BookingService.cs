using Microsoft.Extensions.Logging;
using TravelBookingApp.Core.Interfaces;
using TravelBookingApp.Domain.Models;

namespace TravelBookingApp.Core.Services
{
    public class BookingService : IBookingService
    {
        private readonly IBookingRepository _bookingRepository;
        private readonly IFlightRepository _flightRepository;
        private readonly IHotelRepository _hotelRepository;
        private readonly IRentalCarRepository _carRepository;
        private readonly ILogger<BookingService> _logger;

        public BookingService(
            IBookingRepository bookingRepository,
            IFlightRepository flightRepository,
            IHotelRepository hotelRepository,
            IRentalCarRepository carRepository,
            ILogger<BookingService> logger)
        {
            _bookingRepository = bookingRepository;
            _flightRepository = flightRepository;
            _hotelRepository = hotelRepository;
            _carRepository = carRepository;
            _logger = logger;
        }

        public async Task<IEnumerable<Booking>> GetUserBookingsAsync(Guid userId)
        {
            try
            {
                return await _bookingRepository.GetUserBookingsAsync(userId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving user bookings");
                throw new ApplicationException("Failed to retrieve user bookings", ex);
            }
        }

        public async Task<Booking> GetBookingByIdAsync(Guid id)
        {
            var booking = await _bookingRepository.GetByIdAsync(id);
            if (booking == null)
            {
                throw new KeyNotFoundException($"Booking with ID {id} not found");
            }
            return booking;
        }

        public async Task<bool> CancelBookingAsync(Guid bookingId, Guid userId)
        {
            try
            {
                var booking = await GetBookingByIdAsync(bookingId);

                if (booking.UserId != userId)
                {
                    throw new UnauthorizedAccessException("User is not authorized to cancel this booking");
                }

                if (booking.Status == BookingStatus.Cancelled)
                {
                    throw new InvalidOperationException("Booking is already cancelled");
                }

                // Update booking status
                booking.Status = BookingStatus.Cancelled;
                await _bookingRepository.UpdateAsync(booking);

                // Return the resource to the inventory
                switch (booking.Type)
                {
                    case BookingType.Flight:
                        var flight = await _flightRepository.GetByIdAsync(booking.ItemId);
                        if (flight != null)
                        {
                            flight.AvailableSeats++;
                            await _flightRepository.UpdateAsync(flight);
                        }
                        break;
                    case BookingType.Hotel:
                        var hotel = await _hotelRepository.GetByIdAsync(booking.ItemId);
                        if (hotel != null)
                        {
                            hotel.AvailableRooms++;
                            await _hotelRepository.UpdateAsync(hotel);
                        }
                        break;
                    case BookingType.Car:
                        var car = await _carRepository.GetByIdAsync(booking.ItemId);
                        if (car != null)
                        {
                            car.IsAvailable = true;
                            await _carRepository.UpdateAsync(car);
                        }
                        break;
                }

                return true;
            }
            catch (KeyNotFoundException)
            {
                throw;
            }
            catch (UnauthorizedAccessException)
            {
                throw;
            }
            catch (InvalidOperationException)
            {
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error cancelling booking");
                throw new ApplicationException("Failed to cancel booking", ex);
            }
        }
    }
}
