using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using TravelBookingApp.Core.Interfaces;
using TravelBookingApp.Core.Services;
using TravelBookingApp.Domain.Models;
using Xunit;

namespace TravelBookingApp.UnitTests.Services
{
    public class FlightServiceTests
    {
        private readonly Mock<IFlightRepository> _mockFlightRepository;
        private readonly Mock<IBookingRepository> _mockBookingRepository;
        private readonly Mock<IExternalTravelApiService> _mockExternalApiService;
        private readonly Mock<ILogger<FlightService>> _mockLogger;
        private readonly FlightService _flightService;

        public FlightServiceTests()
        {
            _mockFlightRepository = new Mock<IFlightRepository>();
            _mockBookingRepository = new Mock<IBookingRepository>();
            _mockExternalApiService = new Mock<IExternalTravelApiService>();
            _mockLogger = new Mock<ILogger<FlightService>>();

            _flightService = new FlightService(
                _mockFlightRepository.Object,
                _mockBookingRepository.Object,
                _mockExternalApiService.Object,
                _mockLogger.Object
            );
        }

        [Fact]
        public async Task SearchFlightsAsync_ShouldReturnCombinedResults()
        {
            // Arrange
            var departureCity = "NYC";
            var arrivalCity = "LAX";
            var date = DateTime.Now.AddDays(10);

            var localFlights = new List<Flight>
            {
                new Flight { Id = Guid.NewGuid(), DepartureCity = departureCity, ArrivalCity = arrivalCity }
            };

            var externalFlights = new List<Flight>
            {
                new Flight { Id = Guid.NewGuid(), DepartureCity = departureCity, ArrivalCity = arrivalCity }
            };

            _mockFlightRepository.Setup(r => r.SearchFlightsAsync(departureCity, arrivalCity, date))
                .ReturnsAsync(localFlights);

            _mockExternalApiService.Setup(s => s.SearchFlightsAsync(departureCity, arrivalCity, date))
                .ReturnsAsync(externalFlights);

            // Act
            var result = await _flightService.SearchFlightsAsync(departureCity, arrivalCity, date);

            // Assert
            result.Should().HaveCount(2);
            _mockFlightRepository.Verify(r => r.SearchFlightsAsync(departureCity, arrivalCity, date), Times.Once);
            _mockExternalApiService.Verify(s => s.SearchFlightsAsync(departureCity, arrivalCity, date), Times.Once);
        }

        [Fact]
        public async Task GetFlightByIdAsync_WhenFlightExists_ShouldReturnFlight()
        {
            // Arrange
            var flightId = Guid.NewGuid();
            var flight = new Flight { Id = flightId };

            _mockFlightRepository.Setup(r => r.GetByIdAsync(flightId))
                .ReturnsAsync(flight);

            // Act
            var result = await _flightService.GetFlightByIdAsync(flightId);

            // Assert
            result.Should().Be(flight);
            _mockFlightRepository.Verify(r => r.GetByIdAsync(flightId), Times.Once);
        }

        [Fact]
        public async Task GetFlightByIdAsync_WhenFlightDoesNotExist_ShouldThrowKeyNotFoundException()
        {
            // Arrange
            var flightId = Guid.NewGuid();

            _mockFlightRepository.Setup(r => r.GetByIdAsync(flightId))
                .ReturnsAsync((Flight)null);

            // Act & Assert
            await Assert.ThrowsAsync<KeyNotFoundException>(() => _flightService.GetFlightByIdAsync(flightId));
            _mockFlightRepository.Verify(r => r.GetByIdAsync(flightId), Times.Once);
        }

        [Fact]
        public async Task BookFlightAsync_WhenFlightHasAvailableSeats_ShouldCreateBooking()
        {
            // Arrange
            var flightId = Guid.NewGuid();
            var userId = Guid.NewGuid();
            var bookingDate = DateTime.UtcNow;

            var flight = new Flight
            {
                Id = flightId,
                AvailableSeats = 5,
                Price = 200,
                DepartureTime = DateTime.UtcNow.AddDays(10),
                ArrivalTime = DateTime.UtcNow.AddDays(10).AddHours(5)
            };

            _mockFlightRepository.Setup(r => r.GetByIdAsync(flightId))
                .ReturnsAsync(flight);

            _mockBookingRepository.Setup(r => r.AddAsync(It.IsAny<Booking>()))
                .Returns(Task.CompletedTask);

            _mockFlightRepository.Setup(r => r.UpdateAsync(It.IsAny<Flight>()))
                .Returns(Task.CompletedTask);

            // Act
            var result = await _flightService.BookFlightAsync(flightId, userId, bookingDate);

            // Assert
            result.Should().NotBeNull();
            result.UserId.Should().Be(userId);
            result.ItemId.Should().Be(flightId);
            result.Type.Should().Be(BookingType.Flight);
            result.TotalPrice.Should().Be(200);
            result.Status.Should().Be(BookingStatus.Confirmed);

            _mockBookingRepository.Verify(r => r.AddAsync(It.IsAny<Booking>()), Times.Once);
            _mockFlightRepository.Verify(r => r.UpdateAsync(It.Is<Flight>(f => f.AvailableSeats == 4)), Times.Once);
        }

        [Fact]
        public async Task BookFlightAsync_WhenNoAvailableSeats_ShouldThrowInvalidOperationException()
        {
            // Arrange
            var flightId = Guid.NewGuid();
            var userId = Guid.NewGuid();
            var bookingDate = DateTime.UtcNow;

            var flight = new Flight
            {
                Id = flightId,
                AvailableSeats = 0
            };

            _mockFlightRepository.Setup(r => r.GetByIdAsync(flightId))
                .ReturnsAsync(flight);

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(() =>
                _flightService.BookFlightAsync(flightId, userId, bookingDate));

            _mockBookingRepository.Verify(r => r.AddAsync(It.IsAny<Booking>()), Times.Never);
            _mockFlightRepository.Verify(r => r.UpdateAsync(It.IsAny<Flight>()), Times.Never);
        }
    }
}
