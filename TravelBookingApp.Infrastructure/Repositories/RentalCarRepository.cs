using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TravelBookingApp.Core.Interfaces;
using TravelBookingApp.Domain.Models;
using TravelBookingApp.Infrastructure.Data;

namespace TravelBookingApp.Infrastructure.Repositories
{
    public class RentalCarRepository : IRentalCarRepository
    {
        private readonly ApplicationDbContext _context;

        public RentalCarRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<RentalCar>> SearchCarsAsync(string location, DateTime pickupDate, DateTime returnDate)
        {
            return await _context.RentalCars
                .Where(c => c.Location.ToLower() == location.ToLower() &&
                            c.IsAvailable)
                .ToListAsync();
        }

        public async Task<RentalCar> GetByIdAsync(Guid id)
        {
            return await _context.RentalCars.FindAsync(id);
        }

        public async Task AddAsync(RentalCar car)
        {
            await _context.RentalCars.AddAsync(car);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(RentalCar car)
        {
            _context.RentalCars.Update(car);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(Guid id)
        {
            var car = await GetByIdAsync(id);
            if (car != null)
            {
                _context.RentalCars.Remove(car);
                await _context.SaveChangesAsync();
            }
        }
    }
}
