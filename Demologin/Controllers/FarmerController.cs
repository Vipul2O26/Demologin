using Demologin.Data;
using Microsoft.AspNetCore.Mvc;

namespace Demologin.Controllers
{
    public class FarmerController : Controller
    {
        private readonly ApplicationDbContext _context;

        public FarmerController(ApplicationDbContext context)
        {
            _context = context;
        }

        public IActionResult Dashboard()
        {
            var products = _context.Products.ToList(); // fetch all products
            return View(products); // pass to view
        }
       


    }
}
