using Microsoft.AspNetCore.Mvc;

namespace SignalRApp.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index(string userName)
        {
            ViewData["userName"]=userName;
            return View();
        }
        public IActionResult Chat()
        {
            return View();
        }
    }
}
