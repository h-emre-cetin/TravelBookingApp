using TravelBookingApp.Domain.Models;

namespace TravelBookingApp.Core.Interfaces
{
    public interface IRentalCarRepository
    {
        Task<IEnumerable<RentalCar>> SearchCarsAsync(string location, DateTime pickupDate, DateTime returnDate);
        
        Task<RentalCar> GetByIdAsync(Guid id);
        
        Task AddAsync(RentalCar car);
        
        Task UpdateAsync(RentalCar car);
        
        Task DeleteAsync(Guid id);
    }
}
