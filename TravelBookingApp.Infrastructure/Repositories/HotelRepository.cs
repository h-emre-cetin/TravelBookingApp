using Microsoft.EntityFrameworkCore;
using TravelBookingApp.Core.Interfaces;
using TravelBookingApp.Domain.Models;
using TravelBookingApp.Infrastructure.Data;

namespace TravelBookingApp.Infrastructure.Repositories
{
    public class HotelRepository : IHotelRepository
    {
        private readonly ApplicationDbContext _context;

        public HotelRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Hotel>> SearchHotelsAsync(string city, DateTime checkIn, DateTime checkOut)
        {
            return await _context.Hotels
                .Where(h => h.City.ToLower() == city.ToLower() &&
                            h.AvailableRooms > 0)
                .ToListAsync();
        }

        public async Task<Hotel> GetByIdAsync(Guid id)
        {
            return await _context.Hotels.FindAsync(id);
        }

        public async Task AddAsync(Hotel hotel)
        {
            await _context.Hotels.AddAsync(hotel);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(Hotel hotel)
        {
            _context.Hotels.Update(hotel);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(Guid id)
        {
            var hotel = await GetByIdAsync(id);
            if (hotel != null)
            {
                _context.Hotels.Remove(hotel);
                await _context.SaveChangesAsync();
            }
        }
    }
}
