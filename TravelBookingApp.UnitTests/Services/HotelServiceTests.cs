using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using TravelBookingApp.Core.Interfaces;
using TravelBookingApp.Core.Services;
using TravelBookingApp.Domain.Models;
using Xunit;

namespace TravelBookingApp.UnitTests.Services
{
    public class HotelServiceTests
    {
        private readonly Mock<IHotelRepository> _mockHotelRepository;
        private readonly Mock<IBookingRepository> _mockBookingRepository;
        private readonly Mock<IExternalTravelApiService> _mockExternalApiService;
        private readonly Mock<ILogger<HotelService>> _mockLogger;
        private readonly HotelService _hotelService;

        public HotelServiceTests()
        {
            _mockHotelRepository = new Mock<IHotelRepository>();
            _mockBookingRepository = new Mock<IBookingRepository>();
            _mockExternalApiService = new Mock<IExternalTravelApiService>();
            _mockLogger = new Mock<ILogger<HotelService>>();

            _hotelService = new HotelService(
                _mockHotelRepository.Object,
                _mockBookingRepository.Object,
                _mockExternalApiService.Object,
                _mockLogger.Object
            );
        }

        [Fact]
        public async Task SearchHotelsAsync_ShouldReturnCombinedResults()
        {
            // Arrange
            var city = "New York";
            var checkIn = DateTime.Now.AddDays(10);
            var checkOut = DateTime.Now.AddDays(15);

            var localHotels = new List<Hotel>
            {
                new Hotel { Id = Guid.NewGuid(), City = city, Name = "Local Hotel" }
            };

            var externalHotels = new List<Hotel>
            {
                new Hotel { Id = Guid.NewGuid(), City = city, Name = "External Hotel" }
            };

            _mockHotelRepository.Setup(r => r.SearchHotelsAsync(city, checkIn, checkOut))
                .ReturnsAsync(localHotels);

            _mockExternalApiService.Setup(s => s.SearchHotelsAsync(city, checkIn, checkOut))
                .ReturnsAsync(externalHotels);

            // Act
            var result = await _hotelService.SearchHotelsAsync(city, checkIn, checkOut);

            // Assert
            result.Should().HaveCount(2);
            result.Should().Contain(h => h.Name == "Local Hotel");
            result.Should().Contain(h => h.Name == "External Hotel");
            _mockHotelRepository.Verify(r => r.SearchHotelsAsync(city, checkIn, checkOut), Times.Once);
            _mockExternalApiService.Verify(s => s.SearchHotelsAsync(city, checkIn, checkOut), Times.Once);
        }

        [Fact]
        public async Task GetHotelByIdAsync_WhenHotelExists_ShouldReturnHotel()
        {
            // Arrange
            var hotelId = Guid.NewGuid();
            var hotel = new Hotel { Id = hotelId, Name = "Test Hotel" };

            _mockHotelRepository.Setup(r => r.GetByIdAsync(hotelId))
                .ReturnsAsync(hotel);

            // Act
            var result = await _hotelService.GetHotelByIdAsync(hotelId);

            // Assert
            result.Should().Be(hotel);
            result.Name.Should().Be("Test Hotel");
            _mockHotelRepository.Verify(r => r.GetByIdAsync(hotelId), Times.Once);
        }

        [Fact]
        public async Task GetHotelByIdAsync_WhenHotelDoesNotExist_ShouldThrowKeyNotFoundException()
        {
            // Arrange
            var hotelId = Guid.NewGuid();

            _mockHotelRepository.Setup(r => r.GetByIdAsync(hotelId))
                .ReturnsAsync((Hotel)null);

            // Act & Assert
            await Assert.ThrowsAsync<KeyNotFoundException>(() => _hotelService.GetHotelByIdAsync(hotelId));
            _mockHotelRepository.Verify(r => r.GetByIdAsync(hotelId), Times.Once);
        }

        [Fact]
        public async Task BookHotelAsync_WhenHotelHasAvailableRooms_ShouldCreateBooking()
        {
            // Arrange
            var hotelId = Guid.NewGuid();
            var userId = Guid.NewGuid();
            var checkIn = DateTime.Now.AddDays(10);
            var checkOut = DateTime.Now.AddDays(15);

            var hotel = new Hotel
            {
                Id = hotelId,
                Name = "Test Hotel",
                AvailableRooms = 5,
                PricePerNight = 100
            };

            _mockHotelRepository.Setup(r => r.GetByIdAsync(hotelId))
                .ReturnsAsync(hotel);

            _mockBookingRepository.Setup(r => r.AddAsync(It.IsAny<Booking>()))
                .Returns(Task.CompletedTask);

            _mockHotelRepository.Setup(r => r.UpdateAsync(It.IsAny<Hotel>()))
                .Returns(Task.CompletedTask);

            // Act
            var result = await _hotelService.BookHotelAsync(hotelId, userId, checkIn, checkOut);

            // Assert
            result.Should().NotBeNull();
            result.UserId.Should().Be(userId);
            result.ItemId.Should().Be(hotelId);
            result.Type.Should().Be(BookingType.Hotel);
            result.StartDate.Should().Be(checkIn);
            result.EndDate.Should().Be(checkOut);
            result.TotalPrice.Should().Be(500); // 5 nights * $100
            result.Status.Should().Be(BookingStatus.Confirmed);

            _mockBookingRepository.Verify(r => r.AddAsync(It.IsAny<Booking>()), Times.Once);
            _mockHotelRepository.Verify(r => r.UpdateAsync(It.Is<Hotel>(h => h.AvailableRooms == 4)), Times.Once);
        }

        [Fact]
        public async Task BookHotelAsync_WhenNoAvailableRooms_ShouldThrowInvalidOperationException()
        {
            // Arrange
            var hotelId = Guid.NewGuid();
            var userId = Guid.NewGuid();
            var checkIn = DateTime.Now.AddDays(10);
            var checkOut = DateTime.Now.AddDays(15);

            var hotel = new Hotel
            {
                Id = hotelId,
                AvailableRooms = 0
            };

            _mockHotelRepository.Setup(r => r.GetByIdAsync(hotelId))
                .ReturnsAsync(hotel);

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(() =>
                _hotelService.BookHotelAsync(hotelId, userId, checkIn, checkOut));

            _mockBookingRepository.Verify(r => r.AddAsync(It.IsAny<Booking>()), Times.Never);
            _mockHotelRepository.Verify(r => r.UpdateAsync(It.IsAny<Hotel>()), Times.Never);
        }

        [Fact]
        public async Task BookHotelAsync_WhenInvalidDates_ShouldThrowArgumentException()
        {
            // Arrange
            var hotelId = Guid.NewGuid();
            var userId = Guid.NewGuid();
            var checkIn = DateTime.Now.AddDays(15);
            var checkOut = DateTime.Now.AddDays(10); // Invalid: checkout before checkin

            var hotel = new Hotel
            {
                Id = hotelId,
                AvailableRooms = 5
            };

            _mockHotelRepository.Setup(r => r.GetByIdAsync(hotelId))
                .ReturnsAsync(hotel);

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(() =>
                _hotelService.BookHotelAsync(hotelId, userId, checkIn, checkOut));

            _mockBookingRepository.Verify(r => r.AddAsync(It.IsAny<Booking>()), Times.Never);
            _mockHotelRepository.Verify(r => r.UpdateAsync(It.IsAny<Hotel>()), Times.Never);
        }
    }
}