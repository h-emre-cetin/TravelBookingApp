using TravelBookingApp.Domain.Models;

namespace TravelBookingApp.Core.Interfaces
{
    public interface IHotelService
    {
        Task<IEnumerable<Hotel>> SearchHotelsAsync(string city, DateTime checkIn, DateTime checkOut);

        Task<Hotel> GetHotelByIdAsync(Guid id);

        Task<Booking> BookHotelAsync(Guid hotelId, Guid userId, DateTime checkIn, DateTime checkOut);
    }
}
