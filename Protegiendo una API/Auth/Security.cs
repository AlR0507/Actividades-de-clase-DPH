using System.Security.Cryptography;
using System.Text;

namespace Comprehension.Auth
{
    public static class Security
    {
        public static void CreatePasswordHash(string password, out byte[] hash, out byte[] salt)
        {
            salt = RandomNumberGenerator.GetBytes(16);
            using var pbkdf2 = new Rfc2898DeriveBytes(password, salt, 100_000, HashAlgorithmName.SHA256);
            hash = pbkdf2.GetBytes(32);
        }

        public static bool VerifyPassword(string password, byte[] storedHash, byte[] storedSalt)
        {
            using var pbkdf2 = new Rfc2898DeriveBytes(password, storedSalt, 100_000, HashAlgorithmName.SHA256);
            var computed = pbkdf2.GetBytes(32);
            return CryptographicOperations.FixedTimeEquals(computed, storedHash);
        }

        // 256 bits de entropía. El cliente ve el token base64;
        // en DB guardamos sólo SHA-256 del token.
        public static string CreateBearerToken(out byte[] tokenHash)
        {
            var raw = RandomNumberGenerator.GetBytes(32);
            var token = Convert.ToBase64String(raw);
            using var sha = SHA256.Create();
            tokenHash = sha.ComputeHash(Encoding.UTF8.GetBytes(token));
            return token;
        }

        public static byte[] HashToken(string token)
        {
            using var sha = SHA256.Create();
            return sha.ComputeHash(Encoding.UTF8.GetBytes(token));
        }
    }
}
