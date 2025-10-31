using Microsoft.AspNetCore.Mvc;
using Protegiendo_un_sitio_web.Filters;


namespace Protegiendo_un_sitio_web.Controllers
{
    [RequireAuth]
    public class DashboardController : Controller
    {
        public IActionResult Index()
        {
            // Minimal personal info from context
            ViewBag.Username = HttpContext.Items["Username"]?.ToString();
            ViewBag.FullName = HttpContext.Items["FullName"]?.ToString();
            return View();
        }
    }
}