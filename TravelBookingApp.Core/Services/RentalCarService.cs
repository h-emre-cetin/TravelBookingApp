using Microsoft.Extensions.Logging;
using TravelBookingApp.Core.Interfaces;
using TravelBookingApp.Domain.Models;

namespace TravelBookingApp.Core.Services
{
    public class RentalCarService : IRentalCarService
    {
        private readonly IRentalCarRepository _carRepository;
        private readonly IBookingRepository _bookingRepository;
        private readonly IExternalTravelApiService _externalApiService;
        private readonly ILogger<RentalCarService> _logger;

        public RentalCarService(
            IRentalCarRepository carRepository,
            IBookingRepository bookingRepository,
            IExternalTravelApiService externalApiService,
            ILogger<RentalCarService> logger)
        {
            _carRepository = carRepository;
            _bookingRepository = bookingRepository;
            _externalApiService = externalApiService;
            _logger = logger;
        }

        public async Task<IEnumerable<RentalCar>> SearchCarsAsync(string location, DateTime pickupDate, DateTime returnDate)
        {
            try
            {
                // First check our database
                var localCars = await _carRepository.SearchCarsAsync(location, pickupDate, returnDate);

                // Then check external API
                var externalCars = await _externalApiService.SearchCarsAsync(location, pickupDate, returnDate);

                // Combine results
                return localCars.Concat(externalCars);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error searching for rental cars");
                throw new ApplicationException("Failed to search rental cars", ex);
            }
        }

        public async Task<RentalCar> GetCarByIdAsync(Guid id)
        {
            var car = await _carRepository.GetByIdAsync(id);
            if (car == null)
            {
                throw new KeyNotFoundException($"Rental car with ID {id} not found");
            }
            return car;
        }

        public async Task<Booking> BookCarAsync(Guid carId, Guid userId, DateTime pickupDate, DateTime returnDate)
        {
            var car = await GetCarByIdAsync(carId);

            if (!car.IsAvailable)
            {
                throw new InvalidOperationException("This rental car is not available");
            }

            // Calculate number of days
            int days = (int)(returnDate - pickupDate).TotalDays;
            if (days <= 0)
            {
                throw new ArgumentException("Return date must be after pickup date");
            }

            var booking = new Booking
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                Type = BookingType.Car,
                ItemId = carId,
                BookingDate = DateTime.UtcNow,
                StartDate = pickupDate,
                EndDate = returnDate,
                TotalPrice = car.PricePerDay * days,
                Status = BookingStatus.Confirmed
            };

            await _bookingRepository.AddAsync(booking);

            // Update car availability
            car.IsAvailable = false;
            await _carRepository.UpdateAsync(car);

            return booking;
        }
    }
}
