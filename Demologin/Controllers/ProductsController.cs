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
using Demologin.Data;

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

        // ✅ GET: Products/Details/{id}
        public async Task<IActionResult> Details(Guid? id)
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
                    Id = Guid.NewGuid(),
                    Title = model.Title,
                    Description = model.Description,
                    Price = model.Price,
                    ImageUrl = fileName,
                    UserId = userId,
                    CreatedDate = DateTime.UtcNow
                };

                _context.Add(product);
                await _context.SaveChangesAsync();

                return RedirectToAction(nameof(Index));
            }

            return View(model);
        }

        // ✅ GET: Products/Edit/{id}
        public async Task<IActionResult> Edit(Guid? id)
        {
            if (id == null) return NotFound();

            var product = await _context.Products.FindAsync(id);
            if (product == null) return NotFound();

            // Ownership check
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (product.UserId != userId) return Forbid();

            return View(product);
        }

        // ✅ POST: Products/Edit/{id}
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Guid id, [Bind("Id,Title,Description,Price,Status,ImageUrl,CreatedDate")] Product product)
        {
            if (id != product.Id) return NotFound();

            // Load original product
            var originalProduct = await _context.Products.AsNoTracking()
                                                         .FirstOrDefaultAsync(p => p.Id == id);
            if (originalProduct == null) return NotFound();

            // Ownership check
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (originalProduct.UserId != userId) return Forbid();

            if (ModelState.IsValid)
            {
                try
                {
                    product.UserId = originalProduct.UserId; // preserve UserId
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

        // ✅ GET: Products/Delete/{id}
        public async Task<IActionResult> Delete(Guid? id)
        {
            if (id == null) return NotFound();

            var product = await _context.Products.FirstOrDefaultAsync(m => m.Id == id);
            if (product == null) return NotFound();

            // Ownership check
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (product.UserId != userId) return Forbid();

            return View(product);
        }

        // ✅ POST: Products/Delete/{id}
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(Guid id)
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

        private bool ProductExists(Guid id)
        {
            return _context.Products.Any(e => e.Id == id);
        }
    }
}
