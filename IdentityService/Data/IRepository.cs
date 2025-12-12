using IdentityService.Models;

namespace IdentityService.Data
{
    public interface IRepository
    {
        Task<User?> GetByIdAsync(Guid id);
        Task<User?> GetByEmailAsync(string email);
        Task<User?> GetByPhoneAsync(string phone);
        Task<User> CreateAsync(User user);
        Task<User> UpdateAsync(User user);
        Task<bool> EmailExistsAsync(string email);
        Task<bool> PhoneExistsAsync(string phone);
    }
}
