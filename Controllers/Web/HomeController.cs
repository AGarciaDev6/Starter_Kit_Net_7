using Microsoft.AspNetCore.Mvc;

namespace Starter_NET_7.Controllers.Web
{
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
