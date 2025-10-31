using Microsoft.EntityFrameworkCore;
using Protegiendo_un_sitio_web.Data;
using Protegiendo_un_sitio_web.Models;
using System.Security.Cryptography;


namespace Protegiendo_un_sitio_web.Services
{
    public class SessionService : ISessionService
    {
        private readonly AppDbContext _db;
        public SessionService(AppDbContext db) => _db = db;


        private static string NewToken()
        {
            using var rng = RandomNumberGenerator.Create();
            var bytes = new byte[16]; // 128-bit
            rng.GetBytes(bytes);
            return Convert.ToBase64String(bytes).TrimEnd('=').Replace('+', '-').Replace('/', '_');
        }


        public async Task<UserSession> CreateAsync(int userId)
        {
            var now = DateTime.UtcNow;
            var s = new UserSession
            {
                SessionId = NewToken(),
                UserId = userId,
                CreatedAtUtc = now,
                LastActivityUtc = now,
                IdleTimeoutMinutes = 5,
                IsRevoked = false
            };
            _db.Sessions.Add(s);
            await _db.SaveChangesAsync();
            return s;
        }


        public async Task<User?> ValidateAsync(string sessionId)
        {
            var s = await _db.Sessions.Include(x => x.User).FirstOrDefaultAsync(x => x.SessionId == sessionId);
            if (s == null || s.IsRevoked) return null;
            var now = DateTime.UtcNow;
            var idle = now - s.LastActivityUtc;
            if (idle > TimeSpan.FromMinutes(s.IdleTimeoutMinutes)) return null; // expired by inactivity
            return s.User;
        }


        public async Task TouchAsync(string sessionId)
        {
            var s = await _db.Sessions.FirstOrDefaultAsync(x => x.SessionId == sessionId);
            if (s == null) return;
            var now = DateTime.UtcNow;
            // Update last activity only if at least 60 seconds passed to reduce writes
            if ((now - s.LastActivityUtc) > TimeSpan.FromSeconds(60))
            {
                s.LastActivityUtc = now;
                await _db.SaveChangesAsync();
            }
        }

        public async Task InvalidateAsync(string sessionId)
        {
            var s = await _db.Sessions.FirstOrDefaultAsync(x => x.SessionId == sessionId);
            if (s == null) return;
            s.IsRevoked = true;
            await _db.SaveChangesAsync();
        }
    }
}
