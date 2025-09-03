using System;
using System.IO;
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

            var product = await _context.Products.FirstOrDefaultAsync(m => m.Id == id);
            if (product == null) return NotFound();

            // Ownership check
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (product.UserId != userId) return Forbid();

            return View(product);
        }

        // ✅ Helper to serve images from Uploads folder
        public IActionResult GetImage(string fileName)
        {
            if (string.IsNullOrEmpty(fileName))
                return NotFound();

            var path = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "Uploads", fileName);
            if (!System.IO.File.Exists(path))
                return NotFound();

            var extension = Path.GetExtension(fileName).ToLower();
            var mimeType = extension switch
            {
                ".jpg" or ".jpeg" => "image/jpeg",
                ".png" => "image/png",
                ".gif" => "image/gif",
                _ => "application/octet-stream"
            };

            var fileBytes = System.IO.File.ReadAllBytes(path);
            return File(fileBytes, mimeType);
        }

        // ✅ GET: Products/Create
        public IActionResult Create()
        {
            return View();
        }

        // ✅ POST: Products/Create
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
                    var uploads = Path.Combine(_env.WebRootPath, "Uploads");
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

            var viewModel = new ProductViewModel
            {
                Id = product.Id,
                Title = product.Title,
                Description = product.Description,
                Price = product.Price,
                ImageUrl = product.ImageUrl
            };

            return View(viewModel);
        }

        // ✅ POST: Products/Edit/{id}
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Guid id, ProductViewModel model)
        {
            if (id != model.Id) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    var product = await _context.Products.FindAsync(id);
                    if (product == null) return NotFound();

                    // ✅ Update fields
                    product.Title = model.Title;
                    product.Description = model.Description;
                    product.Price = model.Price;

                    // ✅ Handle new image upload
                    if (model.ImageFile != null && model.ImageFile.Length > 0)
                    {
                        var uploadsFolder = Path.Combine(_env.WebRootPath, "Uploads");
                        Directory.CreateDirectory(uploadsFolder);

                        var fileName = Guid.NewGuid() + Path.GetExtension(model.ImageFile.FileName);
                        var filePath = Path.Combine(uploadsFolder, fileName);

                        using (var stream = new FileStream(filePath, FileMode.Create))
                        {
                            await model.ImageFile.CopyToAsync(stream);
                        }

                        product.ImageUrl = fileName;
                    }

                    _context.Update(product);
                    await _context.SaveChangesAsync();

                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!_context.Products.Any(e => e.Id == model.Id))
                        return NotFound();
                    else
                        throw;
                }
            }

            return View(model);
        }

        // ✅ GET: Products/Delete/{id}
        public async Task<IActionResult> Delete(Guid? id)
        {
            if (id == null) return NotFound();

            var product = await _context.Products.FirstOrDefaultAsync(m => m.Id == id);
            if (product == null) return NotFound();

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
    }
}
