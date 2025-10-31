using Microsoft.AspNetCore.Mvc;


namespace Protegiendo_un_sitio_web.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index() => View();
    }
}