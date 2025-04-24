using TravelBookingApp.Domain.Models;

namespace TravelBookingApp.Core.Interfaces
{
    public interface IExternalTravelApiService
    {
        Task<IEnumerable<Flight>> SearchFlightsAsync(string departureCity, string arrivalCity, DateTime date);
        
        Task<IEnumerable<Hotel>> SearchHotelsAsync(string city, DateTime checkIn, DateTime checkOut);
        
        Task<IEnumerable<RentalCar>> SearchCarsAsync(string location, DateTime pickupDate, DateTime returnDate);
    }
}
