using Microsoft.AspNetCore.Mvc;

namespace Demologin.Controllers
{
    public class AdminController : Controller
    {
        public IActionResult Dashboard()
        {
            return View();  // looks for Views/Admin/Dashboard.cshtml
        }
    }
}
