using TravelBookingApp.Domain.Models;

namespace TravelBookingApp.Core.Interfaces
{
    public interface IFlightRepository
    {
        Task<IEnumerable<Flight>> SearchFlightsAsync(string departureCity, string arrivalCity, DateTime date);

        Task<Flight> GetByIdAsync(Guid id);

        Task UpdateAsync(Flight flight);
    }
}
