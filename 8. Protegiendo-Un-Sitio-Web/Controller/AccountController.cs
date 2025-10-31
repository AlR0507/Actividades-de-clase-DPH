using Microsoft.AspNetCore.Mvc;
using Protegiendo_un_sitio_web.Models;
using Protegiendo_un_sitio_web.Models.ViewModels;
using Protegiendo_un_sitio_web.Services;
using Protegiendo_un_sitio_web.Middleware;


namespace Protegiendo_un_sitio_web.Controllers
{
    public class AccountController : Controller
    {
        private readonly IUserService _users;
        private readonly IPasswordHasher _hasher;
        private readonly ISessionService _sessions;
        public AccountController(IUserService users, IPasswordHasher hasher, ISessionService sessions)
        {
            _users = users; _hasher = hasher; _sessions = sessions;
        }


        [HttpGet]
        public IActionResult Register() => View(new RegisterViewModel());


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterViewModel vm)
        {
            if (!ModelState.IsValid) return View(vm);


            var existing = await _users.FindByUsernameAsync(vm.Username);
            if (existing != null)
            {
                ModelState.AddModelError("Username", "El nombre de usuario ya está en uso.");
                return View(vm);
            }


            var user = new User
            {
                Username = vm.Username,
                Email = vm.Email,
                DateOfBirth = vm.DateOfBirth,
                FullName = vm.FullName,
                PasswordHash = _hasher.Hash(vm.Password)
            };
            await _users.CreateAsync(user);

            // Auto login after registration
            var sess = await _sessions.CreateAsync(user.Id);
            SetSessionCookie(sess.SessionId);
            return RedirectToAction("Index", "Dashboard");
        }


        [HttpGet]
        public IActionResult Login(string? returnUrl = null)
        {
            ViewBag.ReturnUrl = returnUrl;
            return View(new LoginViewModel());
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel vm, string? returnUrl = null)
        {
            if (!ModelState.IsValid) return View(vm);
            var user = await _users.FindByUsernameAsync(vm.Username);
            if (user == null || !_hasher.Verify(vm.Password, user.PasswordHash))
            {
                ModelState.AddModelError(string.Empty, "Credenciales inválidas.");
                return View(vm);
            }



            var sess = await _sessions.CreateAsync(user.Id);
            SetSessionCookie(sess.SessionId);
            if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                return Redirect(returnUrl);
            return RedirectToAction("Index", "Dashboard");
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            var sid = Request.Cookies[SessionUserMiddleware.SessionCookieName];
            if (!string.IsNullOrEmpty(sid))
            {
                await _sessions.InvalidateAsync(sid);
                Response.Cookies.Delete(SessionUserMiddleware.SessionCookieName);
            }
            return RedirectToAction("Index", "Home");
        }


        private void SetSessionCookie(string sessionId)
        {
            Response.Cookies.Append(SessionUserMiddleware.SessionCookieName, sessionId, new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.Lax,
                IsEssential = true,
                Expires = DateTimeOffset.UtcNow.AddMinutes(10) // cookie lifetime; server enforces 5-min idle
            });
        }
    }
}