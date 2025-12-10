using IdentityService.Models.Enums;
using System.Reflection;

namespace IdentityService.Models
{
    public class User : BaseEntity
    {
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string PasswordHash { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public string Gender { get; set; } = string.Empty;
        public string Role { get; set; } = "User";
        public DateTime? LastLoginAt { get; set; }
        public bool EmailVerified { get; set; } = false;
        public List<RefreshToken>? RefreshTokens { get; set; }
    }
}
