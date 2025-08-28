using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Demologin.Models;
using Demologin.ViewModels;
using Microsoft.AspNetCore.Hosting;

namespace Demologin.Controllers
{
    // ✅ Only authenticated Farmers can access this controller
    [Authorize(Roles = "Farmer")]
    public class ProductsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _env;

        public ProductsController(ApplicationDbContext context, IWebHostEnvironment env)
        {
            _context = context;
            _env = env;
        }

        // ✅ GET: Products (only logged-in user's products)
        public async Task<IActionResult> Index()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var userProducts = await _context.Products
                                             .Where(p => p.UserId == userId)
                                             .ToListAsync();

            return View(userProducts);
        }

        // ✅ GET: Products/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var product = await _context.Products
                                        .FirstOrDefaultAsync(m => m.Id == id);

            if (product == null) return NotFound();

            // Ownership check
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (product.UserId != userId) return Forbid();

            return View(product);
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ProductViewModel model)
        {
            if (ModelState.IsValid)
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

                string? fileName = null;
                if (model.ImageFile != null)
                {
                    var uploads = Path.Combine(_env.WebRootPath, "uploads");
                    Directory.CreateDirectory(uploads);

                    fileName = Guid.NewGuid() + Path.GetExtension(model.ImageFile.FileName);
                    var filePath = Path.Combine(uploads, fileName);

                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await model.ImageFile.CopyToAsync(stream);
                    }
                }

                var product = new Product
                {
                    Title = model.Title,
                    Description = model.Description,
                    Price = model.Price,
                    ImageUrl = fileName, // store filename in DB
                    UserId = userId,
                    CreatedDate = DateTime.Now
                };

                _context.Add(product);
                await _context.SaveChangesAsync();

                return RedirectToAction(nameof(Index));
            }

            return View(model);
        }



        // ✅ GET: Products/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var product = await _context.Products.FindAsync(id);
            if (product == null) return NotFound();

            // Ownership check
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (product.UserId != userId) return Forbid();

            return View(product);
        }

        // ✅ POST: Products/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Title,Description,Price,Status,ImageUrl,CreatedDate")] Product product)
        {
            if (id != product.Id) return NotFound();

            // Load original product from DB
            var originalProduct = await _context.Products.AsNoTracking().FirstOrDefaultAsync(p => p.Id == id);
            if (originalProduct == null) return NotFound();

            // Ownership check
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (originalProduct.UserId != userId) return Forbid();

            if (ModelState.IsValid)
            {
                try
                {
                    // Preserve UserId
                    product.UserId = originalProduct.UserId;

                    _context.Update(product);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ProductExists(product.Id))
                        return NotFound();
                    else
                        throw;
                }
                return RedirectToAction(nameof(Index));
            }

            return View(product);
        }

        // ✅ GET: Products/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var product = await _context.Products.FirstOrDefaultAsync(m => m.Id == id);
            if (product == null) return NotFound();

            // Ownership check
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (product.UserId != userId) return Forbid();

            return View(product);
        }

        // ✅ POST: Products/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var product = await _context.Products.FindAsync(id);
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (product != null && product.UserId == userId)
            {
                _context.Products.Remove(product);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
        }

        private bool ProductExists(int id)
        {
            return _context.Products.Any(e => e.Id == id);
        }
    }
}
