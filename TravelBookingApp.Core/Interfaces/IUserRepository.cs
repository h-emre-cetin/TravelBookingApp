﻿using TravelBookingApp.Domain.Models;

namespace TravelBookingApp.Core.Interfaces
{
    public interface IUserRepository
    {
        Task<User> GetByIdAsync(Guid id);
       
        Task<User> GetByEmailAsync(string email);
        
        Task AddAsync(User user);
        
        Task UpdateAsync(User user);
        
        Task DeleteAsync(Guid id);
    }
}
