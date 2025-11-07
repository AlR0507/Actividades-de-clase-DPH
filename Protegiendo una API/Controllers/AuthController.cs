using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Comprehension.Data;
using Comprehension.Models;
using Comprehension.Auth;

namespace Comprehension.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly ComprehensionContext _db;
        public AuthController(ComprehensionContext db) => _db = db;

        public record RegisterDto(string UserName, string Password, string? Role = null);
        public record LoginDto(string UserName, string Password);
        public record TokenDto(string Token, DateTime ExpiresAtUtc);

        [HttpPost("register")]
        public async Task<IActionResult> Register(RegisterDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.UserName) || string.IsNullOrWhiteSpace(dto.Password))
                return BadRequest("Usuario y contraseña son obligatorios.");

            if (await _db.User.AnyAsync(u => u.UserName == dto.UserName))
                return Conflict("El usuario ya existe.");

            Security.CreatePasswordHash(dto.Password, out var hash, out var salt);

            var user = new User
            {
                Id = Guid.NewGuid(),
                UserName = dto.UserName,
                PasswordHash = hash,
                PasswordSalt = salt,
                Role = Enum.TryParse<AppRole>(dto.Role ?? "", true, out var r) ? r : AppRole.User
            };

            _db.User.Add(user);
            await _db.SaveChangesAsync();
            return Created(string.Empty, new { user.Id, user.UserName, user.Role });
        }

        [HttpPost("login")]
        public async Task<ActionResult<TokenDto>> Login(LoginDto dto)
        {
            var user = await _db.User.FirstOrDefaultAsync(u => u.UserName == dto.UserName);
            if (user is null || !Security.VerifyPassword(dto.Password, user.PasswordHash, user.PasswordSalt))
                return Unauthorized();

            var token = Security.CreateBearerToken(out var tokenHash);
            var now = DateTime.UtcNow;
            var st = new SessionToken
            {
                Id = Guid.NewGuid(),
                TokenHash = tokenHash,
                IssuedAtUtc = now,
                ExpiresAtUtc = now.AddMinutes(60),
                UserId = user.Id,
                User = user
            };

            _db.SessionToken.Add(st);
            await _db.SaveChangesAsync();

            return new TokenDto(token, st.ExpiresAtUtc);
        }

        [HttpPost("logout")]
        public async Task<IActionResult> Logout([FromHeader(Name = "Authorization")] string? auth)
        {
            if (string.IsNullOrWhiteSpace(auth) || !auth.StartsWith("Bearer "))
                return Unauthorized();

            var token = auth["Bearer ".Length..].Trim();
            var hash = Security.HashToken(token);

            var st = await _db.SessionToken.FirstOrDefaultAsync(t => t.TokenHash == hash && !t.Revoked);
            if (st is null) return Ok(); // idempotente

            st.Revoked = true;
            await _db.SaveChangesAsync();
            return Ok();
        }
    }
}

