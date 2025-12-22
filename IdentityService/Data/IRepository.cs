using IdentityService.Models;

namespace IdentityService.Data
{
    public interface IRepository
    {
        // User operations
        Task<User?> GetByIdAsync(Guid id);
        Task<User?> GetByEmailAsync(string email);
        Task<User?> GetByPhoneAsync(string phone);
        Task<User> CreateAsync(User user);
        Task<User> UpdateAsync(User user);
        Task<bool> EmailExistsAsync(string email);
        Task<bool> PhoneExistsAsync(string phone);

        // PasswordResetRequest operations
        Task<PasswordResetRequest> CreatePasswordResetRequestAsync(PasswordResetRequest request);
        Task<PasswordResetRequest?> GetPasswordResetRequestByIdAsync(Guid id);
        Task<PasswordResetRequest?> GetPasswordResetRequestByEmailAndCodeAsync(string email, string code);
        Task<List<PasswordResetRequest>> GetActivePasswordResetRequestsByEmailAsync(string email);
        Task<PasswordResetRequest> UpdatePasswordResetRequestAsync(PasswordResetRequest request);
        Task<int> InvalidatePasswordResetRequestsAsync(string email);

    }
}
