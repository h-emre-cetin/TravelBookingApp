using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using TravelBookingApp.Core.Interfaces;
using TravelBookingApp.Core.Services;
using TravelBookingApp.Domain.Models;
using Xunit;

namespace TravelBookingApp.UnitTests.Services
{
    public class RentalCarServiceTests
    {
        private readonly Mock<IRentalCarRepository> _mockCarRepository;
        private readonly Mock<IBookingRepository> _mockBookingRepository;
        private readonly Mock<IExternalTravelApiService> _mockExternalApiService;
        private readonly Mock<ILogger<RentalCarService>> _mockLogger;
        private readonly RentalCarService _carService;

        public RentalCarServiceTests()
        {
            _mockCarRepository = new Mock<IRentalCarRepository>();
            _mockBookingRepository = new Mock<IBookingRepository>();
            _mockExternalApiService = new Mock<IExternalTravelApiService>();
            _mockLogger = new Mock<ILogger<RentalCarService>>();

            _carService = new RentalCarService(
                _mockCarRepository.Object,
                _mockBookingRepository.Object,
                _mockExternalApiService.Object,
                _mockLogger.Object
            );
        }

        [Fact]
        public async Task SearchCarsAsync_ShouldReturnCombinedResults()
        {
            // Arrange
            var location = "New York";
            var pickupDate = DateTime.Now.AddDays(10);
            var returnDate = DateTime.Now.AddDays(15);

            var localCars = new List<RentalCar>
            {
                new RentalCar { Id = Guid.NewGuid(), Location = location, Make = "Toyota", Model = "Corolla" }
            };

            var externalCars = new List<RentalCar>
            {
                new RentalCar { Id = Guid.NewGuid(), Location = location, Make = "Honda", Model = "Civic" }
            };

            _mockCarRepository.Setup(r => r.SearchCarsAsync(location, pickupDate, returnDate))
                .ReturnsAsync(localCars);

            _mockExternalApiService.Setup(s => s.SearchCarsAsync(location, pickupDate, returnDate))
                .ReturnsAsync(externalCars);

            // Act
            var result = await _carService.SearchCarsAsync(location, pickupDate, returnDate);

            // Assert
            result.Should().HaveCount(2);
            result.Should().Contain(c => c.Make == "Toyota" && c.Model == "Corolla");
            result.Should().Contain(c => c.Make == "Honda" && c.Model == "Civic");
            _mockCarRepository.Verify(r => r.SearchCarsAsync(location, pickupDate, returnDate), Times.Once);
            _mockExternalApiService.Verify(s => s.SearchCarsAsync(location, pickupDate, returnDate), Times.Once);
        }

        [Fact]
        public async Task GetCarByIdAsync_WhenCarExists_ShouldReturnCar()
        {
            // Arrange
            var carId = Guid.NewGuid();
            var car = new RentalCar { Id = carId, Make = "Toyota", Model = "Corolla" };

            _mockCarRepository.Setup(r => r.GetByIdAsync(carId))
                .ReturnsAsync(car);

            // Act
            var result = await _carService.GetCarByIdAsync(carId);

            // Assert
            result.Should().Be(car);
            result.Make.Should().Be("Toyota");
            result.Model.Should().Be("Corolla");
            _mockCarRepository.Verify(r => r.GetByIdAsync(carId), Times.Once);
        }

        [Fact]
        public async Task GetCarByIdAsync_WhenCarDoesNotExist_ShouldThrowKeyNotFoundException()
        {
            // Arrange
            var carId = Guid.NewGuid();

            _mockCarRepository.Setup(r => r.GetByIdAsync(carId))
                .ReturnsAsync((RentalCar)null);

            // Act & Assert
            await Assert.ThrowsAsync<KeyNotFoundException>(() => _carService.GetCarByIdAsync(carId));
            _mockCarRepository.Verify(r => r.GetByIdAsync(carId), Times.Once);
        }

        [Fact]
        public async Task BookCarAsync_WhenCarIsAvailable_ShouldCreateBooking()
        {
            // Arrange
            var carId = Guid.NewGuid();
            var userId = Guid.NewGuid();
            var pickupDate = DateTime.Now.AddDays(10);
            var returnDate = DateTime.Now.AddDays(15);

            var car = new RentalCar
            {
                Id = carId,
                Make = "Toyota",
                Model = "Corolla",
                IsAvailable = true,
                PricePerDay = 50
            };

            _mockCarRepository.Setup(r => r.GetByIdAsync(carId))
                .ReturnsAsync(car);

            _mockBookingRepository.Setup(r => r.AddAsync(It.IsAny<Booking>()))
                .Returns(Task.CompletedTask);

            _mockCarRepository.Setup(r => r.UpdateAsync(It.IsAny<RentalCar>()))
                .Returns(Task.CompletedTask);

            // Act
            var result = await _carService.BookCarAsync(carId, userId, pickupDate, returnDate);

            // Assert
            result.Should().NotBeNull();
            result.UserId.Should().Be(userId);
            result.ItemId.Should().Be(carId);
            result.Type.Should().Be(BookingType.Car);
            result.StartDate.Should().Be(pickupDate);
            result.EndDate.Should().Be(returnDate);
            result.TotalPrice.Should().Be(250); // 5 days * $50
            result.Status.Should().Be(BookingStatus.Confirmed);

            _mockBookingRepository.Verify(r => r.AddAsync(It.IsAny<Booking>()), Times.Once);
            _mockCarRepository.Verify(r => r.UpdateAsync(It.Is<RentalCar>(c => c.IsAvailable == false)), Times.Once);
        }

        [Fact]
        public async Task BookCarAsync_WhenCarIsNotAvailable_ShouldThrowInvalidOperationException()
        {
            // Arrange
            var carId = Guid.NewGuid();
            var userId = Guid.NewGuid();
            var pickupDate = DateTime.Now.AddDays(10);
            var returnDate = DateTime.Now.AddDays(15);

            var car = new RentalCar
            {
                Id = carId,
                IsAvailable = false
            };

            _mockCarRepository.Setup(r => r.GetByIdAsync(carId))
                .ReturnsAsync(car);

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(() =>
                _carService.BookCarAsync(carId, userId, pickupDate, returnDate));

            _mockBookingRepository.Verify(r => r.AddAsync(It.IsAny<Booking>()), Times.Never);
            _mockCarRepository.Verify(r => r.UpdateAsync(It.IsAny<RentalCar>()), Times.Never);
        }

        [Fact]
        public async Task BookCarAsync_WhenInvalidDates_ShouldThrowArgumentException()
        {
            // Arrange
            var carId = Guid.NewGuid();
            var userId = Guid.NewGuid();
            var pickupDate = DateTime.Now.AddDays(15);
            var returnDate = DateTime.Now.AddDays(10); // Invalid: return before pickup

            var car = new RentalCar
            {
                Id = carId,
                IsAvailable = true
            };

            _mockCarRepository.Setup(r => r.GetByIdAsync(carId))
                .ReturnsAsync(car);

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(() =>
                _carService.BookCarAsync(carId, userId, pickupDate, returnDate));

            _mockBookingRepository.Verify(r => r.AddAsync(It.IsAny<Booking>()), Times.Never);
            _mockCarRepository.Verify(r => r.UpdateAsync(It.IsAny<RentalCar>()), Times.Never);
        }
    }
}