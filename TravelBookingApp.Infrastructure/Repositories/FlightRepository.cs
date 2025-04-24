using Microsoft.EntityFrameworkCore;
using TravelBookingApp.Core.Interfaces;
using TravelBookingApp.Domain.Models;
using TravelBookingApp.Infrastructure.Data;

namespace TravelBookingApp.Infrastructure.Repositories
{
    public class FlightRepository : IFlightRepository
    {
        private readonly ApplicationDbContext _context;

        public FlightRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Flight>> SearchFlightsAsync(string departureCity, string arrivalCity, DateTime date)
        {
            try
            {
                return await _context.Flights
                .Where(f => f.DepartureCity.ToLower() == departureCity.ToLower() &&
                            f.ArrivalCity.ToLower() == arrivalCity.ToLower() &&
                            f.DepartureTime.Date == date.Date &&
                            f.AvailableSeats > 0)
                .ToListAsync();
            }
            catch (Exception)
            {

                throw;
            }
            
        }

        public async Task<Flight> GetByIdAsync(Guid id)
        {
            return await _context.Flights.FindAsync(id);
        }

        public async Task UpdateAsync(Flight flight)
        {
            _context.Flights.Update(flight);
            await _context.SaveChangesAsync();
        }
    }
}
