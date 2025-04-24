using TravelBookingApp.Domain.Models;

namespace TravelBookingApp.Core.Interfaces
{
    public interface IFlightService
    {
        Task<IEnumerable<Flight>> SearchFlightsAsync(string departureCity, string arrivalCity, DateTime date);
        
        Task<Flight> GetFlightByIdAsync(Guid id);
        
        Task<Booking> BookFlightAsync(Guid flightId, Guid userId, DateTime bookingDate);
    }
}
