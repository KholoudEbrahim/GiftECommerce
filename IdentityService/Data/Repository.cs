using IdentityService.Events;
using IdentityService.Models;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using System;

namespace IdentityService.Data
{
    public class Repository : IRepository
    {
        private readonly IdentityDbContext _context;
        private readonly IPublishEndpoint _publishEndpoint;
        private readonly ILogger<Repository> _logger;
        public Repository(IdentityDbContext context, IPublishEndpoint publishEndpoint, ILogger<Repository> logger)
        {
            _context = context;
            _publishEndpoint = publishEndpoint;
            _logger = logger;
        }

        public async Task<User?> GetByIdAsync(Guid id)
        {
            return await _context.Users.FindAsync(id);
        }

        public async Task<User?> GetByEmailAsync(string email)
        {
            return await _context.Users
                .FirstOrDefaultAsync(u => u.Email.ToLower() == email.ToLower());
        }

        public async Task<User?> GetByPhoneAsync(string phone)
        {
            return await _context.Users
                .FirstOrDefaultAsync(u => u.Phone == phone);
        }

        public async Task<User> CreateAsync(User user)
        {
            try
            {
 
                await _context.Users.AddAsync(user);
                await _context.SaveChangesAsync();
                await PublishUserCreatedEvent(user);

                return user;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating user in repository");
                throw;
            }
        }

        private async Task PublishUserCreatedEvent(User user)
        {
            try
            {
                var @event = new UserCreatedEvent
                {
                    UserId = user.Id,
                    Email = user.Email,
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    Phone = user.Phone,
                    Gender = user.Gender,
                    CreatedAt = DateTime.UtcNow
                };

                await _publishEndpoint.Publish(@event);

                _logger.LogInformation(
                    " UserCreatedEvent published from repository for: {Email}",
                    user.Email);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, " Failed to publish UserCreatedEvent for: {Email}", user.Email);
          
            }
        }


        public async Task<User> UpdateAsync(User user)
        {
            _context.Users.Update(user);
            await _context.SaveChangesAsync();
            return user;
        }

        public async Task<bool> EmailExistsAsync(string email)
        {
            return await _context.Users
                .AnyAsync(u => u.Email.ToLower() == email.ToLower());
        }

        public async Task<bool> PhoneExistsAsync(string phone)
        {
            return await _context.Users
                .AnyAsync(u => u.Phone == phone);
        }
    }
}