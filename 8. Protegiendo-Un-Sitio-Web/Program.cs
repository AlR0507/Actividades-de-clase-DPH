using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Text;
using Protegiendo_un_sitio_web.Data;
using Protegiendo_un_sitio_web.Middleware;
using Protegiendo_un_sitio_web.Services;


var builder = WebApplication.CreateBuilder(args);


// EF Core (replace with your connection string)
builder.Services.AddDbContext<AppDbContext>(options =>
options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));


// MVC only (no AddAuthentication, no AddAuthorization, no AddSession)
builder.Services.AddControllersWithViews();


// Custom services (our own auth/session logic)
builder.Services.AddSingleton<IPasswordHasher, PasswordHasher>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<ISessionService, SessionService>();


// Anti-forgery is NOT authentication; it's okay to keep for CSRF protection
builder.Services.AddAntiforgery();


var app = builder.Build();


if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}


app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();


// IMPORTANT: We do NOT call UseAuthentication/UseAuthorization/UseSession.
// We rely solely on our custom SessionUserMiddleware + DB-backed sessions.
app.UseMiddleware<SessionUserMiddleware>();


app.MapControllerRoute(
name: "default",
pattern: "{controller=Home}/{action=Index}/{id?}");


app.Run();