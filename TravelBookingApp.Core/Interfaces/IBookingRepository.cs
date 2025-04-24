using TravelBookingApp.Domain.Models;

namespace TravelBookingApp.Core.Interfaces
{
    public interface IBookingRepository
    {
        Task<IEnumerable<Booking>> GetUserBookingsAsync(Guid userId);

        Task<Booking> GetByIdAsync(Guid id);

        Task AddAsync(Booking booking);

        Task UpdateAsync(Booking booking);

        Task DeleteAsync(Guid id);
    }
}
