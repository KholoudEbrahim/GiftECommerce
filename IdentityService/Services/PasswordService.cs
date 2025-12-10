using Microsoft.AspNetCore.Cryptography.KeyDerivation;
using System.Security.Cryptography;

namespace IdentityService.Services
{
    public class PasswordService : IPasswordService
    {
        private const int SaltSize = 128 / 8; 
        private const int HashSize = 256 / 8; 
        private const int Iterations = 10000;
        private const KeyDerivationPrf Prf = KeyDerivationPrf.HMACSHA256;

        public string HashPassword(string password)
        {
            var salt = RandomNumberGenerator.GetBytes(SaltSize);

            var hash = KeyDerivation.Pbkdf2(
                password: password,
                salt: salt,
                prf: Prf,
                iterationCount: Iterations,
                numBytesRequested: HashSize
            );

            var hashBytes = new byte[SaltSize + HashSize];
            Array.Copy(salt, 0, hashBytes, 0, SaltSize);
            Array.Copy(hash, 0, hashBytes, SaltSize, HashSize);

            return Convert.ToBase64String(hashBytes);
        }

        public bool VerifyPassword(string password, string passwordHash)
        {
            var hashBytes = Convert.FromBase64String(passwordHash);

            var salt = new byte[SaltSize];
            Array.Copy(hashBytes, 0, salt, 0, SaltSize);

            var storedHash = new byte[HashSize];
            Array.Copy(hashBytes, SaltSize, storedHash, 0, HashSize);

            var computedHash = KeyDerivation.Pbkdf2(
                password: password,
                salt: salt,
                prf: Prf,
                iterationCount: Iterations,
                numBytesRequested: HashSize
            );

            return CryptographicOperations.FixedTimeEquals(computedHash, storedHash);
        }
    }

}
