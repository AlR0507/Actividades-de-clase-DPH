using System.Security.Cryptography;
using System.Text;


namespace Protegiendo_un_sitio_web.Services
{
    public class PasswordHasher : IPasswordHasher
    {
        public string Hash(string password, int iterations = 100_000)
        {
            using var rng = RandomNumberGenerator.Create();
            var salt = new byte[16]; // 128-bit salt
            rng.GetBytes(salt);
            using var pbkdf2 = new Rfc2898DeriveBytes(password, salt, iterations, HashAlgorithmName.SHA256);
            var hash = pbkdf2.GetBytes(32); // 256-bit hash
            return $"{iterations}.{Base64UrlEncode(salt)}.{Base64UrlEncode(hash)}";
        }


        public bool Verify(string password, string stored)
        {
            var parts = stored.Split('.');
            if (parts.Length != 3) return false;
            if (!int.TryParse(parts[0], out var iter)) return false;
            var salt = Base64UrlDecode(parts[1]);
            var hash = Base64UrlDecode(parts[2]);
            using var pbkdf2 = new Rfc2898DeriveBytes(password, salt, iter, HashAlgorithmName.SHA256);
            var computed = pbkdf2.GetBytes(hash.Length);
            return CryptographicOperations.FixedTimeEquals(computed, hash);
        }


        static string Base64UrlEncode(byte[] bytes) => Convert.ToBase64String(bytes).TrimEnd('=').Replace('+', '-').Replace('/', '_');
        static byte[] Base64UrlDecode(string s)
        {
            s = s.Replace('-', '+').Replace('_', '/');
            switch (s.Length % 4) { case 2: s += "=="; break; case 3: s += "="; break; }
            return Convert.FromBase64String(s);
        }
    }
}