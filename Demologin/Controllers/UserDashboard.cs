using Microsoft.AspNetCore.Mvc;

namespace Demologin.Controllers
{
    public class UserDashboard : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
