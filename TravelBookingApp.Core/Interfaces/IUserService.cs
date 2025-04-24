using TravelBookingApp.Domain.Models;

namespace TravelBookingApp.Core.Interfaces
{
    public interface IUserService
    {
        Task<User> GetUserByIdAsync(Guid id);
        
        Task<User> GetUserByEmailAsync(string email);
        
        Task<User> RegisterUserAsync(string email, string password, string firstName, string lastName);
        
        Task<bool> AuthenticateAsync(string email, string password);
    }
}
