using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TravelBookingApp.Domain.Models;

namespace TravelBookingApp.Core.Interfaces
{
    public interface IRentalCarService
    {
        Task<IEnumerable<RentalCar>> SearchCarsAsync(string location, DateTime pickupDate, DateTime returnDate);
        
        Task<RentalCar> GetCarByIdAsync(Guid id);
        
        Task<Booking> BookCarAsync(Guid carId, Guid userId, DateTime pickupDate, DateTime returnDate);
    }
}
