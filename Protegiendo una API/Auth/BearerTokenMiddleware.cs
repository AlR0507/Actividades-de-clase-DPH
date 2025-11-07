using Comprehension.Data;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace Comprehension.Auth
{
    public class BearerTokenMiddleware
    {
        private readonly RequestDelegate _next;
        public BearerTokenMiddleware(RequestDelegate next) => _next = next;

        public async Task Invoke(HttpContext ctx, ComprehensionContext db)
        {
            var header = ctx.Request.Headers.Authorization.ToString();
            if (!string.IsNullOrWhiteSpace(header) && header.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
            {
                var token = header["Bearer ".Length..].Trim();
                var hash = Security.HashToken(token);
                var now = DateTime.UtcNow;

                var st = await db.SessionToken
                                 .Include(t => t.User)
                                 .FirstOrDefaultAsync(t => !t.Revoked &&
                                                           t.ExpiresAtUtc > now &&
                                                           t.TokenHash == hash);

                if (st != null)
                {
                    var claims = new[]
                    {
                        new Claim(ClaimTypes.NameIdentifier, st.User.Id.ToString()),
                        new Claim(ClaimTypes.Name, st.User.UserName),
                        new Claim(ClaimTypes.Role, st.User.Role.ToString())
                    };
                    ctx.User = new ClaimsPrincipal(new ClaimsIdentity(claims, "Bearer"));
                }
            }

            await _next(ctx);
        }
    }
}
