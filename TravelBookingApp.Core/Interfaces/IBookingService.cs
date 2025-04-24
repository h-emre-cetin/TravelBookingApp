using TravelBookingApp.Domain.Models;

namespace TravelBookingApp.Core.Interfaces
{
    public interface IBookingService
    {
        Task<IEnumerable<Booking>> GetUserBookingsAsync(Guid userId);
        
        Task<Booking> GetBookingByIdAsync(Guid id);
        
        Task<bool> CancelBookingAsync(Guid bookingId, Guid userId);
    }
}
