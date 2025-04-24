using TravelBookingApp.Domain.Models;

namespace TravelBookingApp.Core.Interfaces
{
    public interface IHotelRepository
    {
        Task<IEnumerable<Hotel>> SearchHotelsAsync(string city, DateTime checkIn, DateTime checkOut);

        Task<Hotel> GetByIdAsync(Guid id);

        Task AddAsync(Hotel hotel);

        Task UpdateAsync(Hotel hotel);

        Task DeleteAsync(Guid id);
    }
}
