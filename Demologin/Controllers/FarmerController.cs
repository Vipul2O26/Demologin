using Microsoft.AspNetCore.Mvc;

namespace Demologin.Controllers
{
    public class FarmerController : Controller
    {
        public IActionResult Dashboard()
        {
            return View(); 
        }
    }
}
