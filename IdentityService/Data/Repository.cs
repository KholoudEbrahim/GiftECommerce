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


                return user;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating user in repository");
                throw;
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
                .AsNoTracking()
                .AnyAsync(u => u.Email == email);
        }

        public async Task<bool> PhoneExistsAsync(string phone)
        {
            return await _context.Users
                .AsNoTracking()
                .AnyAsync(u => u.Phone == phone);
        }

        public async Task<PasswordResetRequest> CreatePasswordResetRequestAsync(PasswordResetRequest request)
        {
            try
            {
                await _context.PasswordResetRequests.AddAsync(request);
                await _context.SaveChangesAsync();
                return request;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating password reset request for {Email}", request.Email);
                throw;
            }
        }

        public async Task<PasswordResetRequest?> GetPasswordResetRequestByIdAsync(Guid id)
        {
            return await _context.PasswordResetRequests
                .FirstOrDefaultAsync(r => r.Id == id && r.IsActive);
        }

        public async Task<PasswordResetRequest?> GetPasswordResetRequestByEmailAndCodeAsync(string email, string code)
        {
            return await _context.PasswordResetRequests
                .Where(r => r.Email.ToLower() == email.ToLower()
                         && r.ResetCode == code
                         && !r.IsUsed
                         && r.IsActive)
                .OrderByDescending(r => r.CreatedAt)
                .FirstOrDefaultAsync();
        }

        public async Task<List<PasswordResetRequest>> GetActivePasswordResetRequestsByEmailAsync(string email)
        {
            return await _context.PasswordResetRequests
                .Where(r => r.Email.ToLower() == email.ToLower()
                         && !r.IsUsed
                         && r.IsActive)
                .ToListAsync();
        }

        public async Task<PasswordResetRequest> UpdatePasswordResetRequestAsync(PasswordResetRequest request)
        {
            _context.PasswordResetRequests.Update(request);
            await _context.SaveChangesAsync();
            return request;
        }

        public async Task<int> InvalidatePasswordResetRequestsAsync(string email)
        {
            var requests = await GetActivePasswordResetRequestsByEmailAsync(email);

            foreach (var request in requests)
            {
                request.IsUsed = true;
                request.UsedAt = DateTime.UtcNow;
                request.UpdatedAt = DateTime.UtcNow;
            }

            return await _context.SaveChangesAsync();
        }
        public async Task<List<RefreshToken>> GetActiveRefreshTokensByUserIdAsync(Guid userId)
        {
            return await _context.RefreshTokens
                .Where(rt => rt.UserId == userId
                    && !rt.IsRevoked
                    && rt.ExpiresAt > DateTime.UtcNow
                    && rt.IsActive)
                .ToListAsync();
        }

        public async Task<int> RevokeAllRefreshTokensForUserAsync(Guid userId)
        {
            var tokens = await GetActiveRefreshTokensByUserIdAsync(userId);

            foreach (var token in tokens)
            {
                token.IsRevoked = true;
                token.UpdatedAt = DateTime.UtcNow;
            }

            return await _context.SaveChangesAsync();
        }

        public async Task<int> CleanupExpiredRefreshTokensAsync()
        {
            var expiredTokens = await _context.RefreshTokens
                .Where(rt => rt.ExpiresAt <= DateTime.UtcNow
                    || (rt.IsRevoked && rt.UpdatedAt <= DateTime.UtcNow.AddDays(-30)))
                .ToListAsync();

            _context.RefreshTokens.RemoveRange(expiredTokens);

            return await _context.SaveChangesAsync();
        }
    }

}