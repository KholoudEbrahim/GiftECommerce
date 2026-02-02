using System.Security.Cryptography;

namespace IdentityService.Services
{
    public static class ResetCodeGenerator
    {
        public static string Generate(int length = 6)
        {
            var bytes = RandomNumberGenerator.GetBytes(4);
            var value = BitConverter.ToUInt32(bytes, 0) % (uint)Math.Pow(10, length);
            return value.ToString($"D{length}");
        }
    }
}
