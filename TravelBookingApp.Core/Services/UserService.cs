using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TravelBookingApp.Core.Interfaces;
using TravelBookingApp.Domain.Models;

namespace TravelBookingApp.Core.Services
{
    public class UserService : IUserService
    {
        private readonly IUserRepository _userRepository;
        private readonly ILogger<UserService> _logger;

        public UserService(IUserRepository userRepository, ILogger<UserService> logger)
        {
            _userRepository = userRepository;
            _logger = logger;
        }

        public async Task<User> GetUserByIdAsync(Guid id)
        {
            var user = await _userRepository.GetByIdAsync(id);
            if (user == null)
            {
                throw new KeyNotFoundException($"User with ID {id} not found");
            }
            return user;
        }

        public async Task<User> GetUserByEmailAsync(string email)
        {
            var user = await _userRepository.GetByEmailAsync(email);
            if (user == null)
            {
                throw new KeyNotFoundException($"User with email {email} not found");
            }
            return user;
        }

        public async Task<User> RegisterUserAsync(string email, string password, string firstName, string lastName)
        {
            try
            {
                // Check if user already exists
                var existingUser = await _userRepository.GetByEmailAsync(email);
                if (existingUser != null)
                {
                    throw new InvalidOperationException($"User with email {email} already exists");
                }

                // Hash the password (in a real app, use a proper password hasher)
                string passwordHash = BCrypt.Net.BCrypt.HashPassword(password);

                var user = new User
                {
                    Id = Guid.NewGuid(),
                    Email = email,
                    FirstName = firstName,
                    LastName = lastName,
                    PasswordHash = passwordHash
                };

                await _userRepository.AddAsync(user);
                return user;
            }
            catch (InvalidOperationException)
            {
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error registering user");
                throw new ApplicationException("Failed to register user", ex);
            }
        }

        public async Task<bool> AuthenticateAsync(string email, string password)
        {
            try
            {
                var user = await _userRepository.GetByEmailAsync(email);
                if (user == null)
                {
                    return false;
                }

                // Verify password (in a real app, use a proper password hasher)
                return BCrypt.Net.BCrypt.Verify(password, user.PasswordHash);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error authenticating user");
                throw new ApplicationException("Failed to authenticate user", ex);
            }
        }
    }
}
