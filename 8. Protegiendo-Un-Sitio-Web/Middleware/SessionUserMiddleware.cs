using Protegiendo_un_sitio_web.Services;


namespace Protegiendo_un_sitio_web.Middleware
{
    public class SessionUserMiddleware
    {
        private readonly RequestDelegate _next;
        public const string SessionCookieName = "app.sid";
        public SessionUserMiddleware(RequestDelegate next) { _next = next; }


        public async Task Invoke(HttpContext ctx, ISessionService sessions)
        {
            string? sid = ctx.Request.Cookies[SessionCookieName];
            if (!string.IsNullOrWhiteSpace(sid))
            {
                var user = await sessions.ValidateAsync(sid);
                if (user != null)
                {
                    ctx.Items["UserId"] = user.Id;
                    ctx.Items["Username"] = user.Username;
                    ctx.Items["FullName"] = user.FullName;
                    await sessions.TouchAsync(sid); // sliding window
                }
            }
            await _next(ctx);
        }
    }
}