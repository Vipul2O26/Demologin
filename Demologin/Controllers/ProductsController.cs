using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims; // <-- Add this to get the user's ID
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization; // <-- Add this for role-based access
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Demologin.Models;

namespace Demologin.Controllers
{
    // Restrict access to only authenticated users with the "Farmer" role
    [Authorize(Roles = "Farmer")]
    public class ProductsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ProductsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Products (Shows ONLY the logged-in user's products)
        public async Task<IActionResult> Index()
        {
            // Get the ID of the currently logged-in user
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            // Filter products to show only the ones owned by the current user
            var userProducts = await _context.Products
                                             .Where(p => p.UserId == userId)
                                             .ToListAsync();

            return View(userProducts);
        }

        // GET: Products/Details/5 (Checks for ownership)
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var product = await _context.Products
                                        .Include(p => p.User)
                                        .FirstOrDefaultAsync(m => m.Id == id);

            if (product == null)
            {
                return NotFound();
            }

            // Security Check: Only the owner can view their product details
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (product.UserId != userId)
            {
                return Forbid(); // Return 403 Forbidden
            }

            return View(product);
        }

        // GET: Products/Create
        public IActionResult Create()
        {
            // We no longer need this line as the UserId is automatically assigned
            // ViewData["UserId"] = new SelectList(_context.Users, "Id", "Id");
            return View();
        }

        // POST: Products/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        // BIND only the properties the user can input to prevent overposting
        public async Task<IActionResult> Create([Bind("Title,Description,Price")] Product product)
        {
            // Get the ID of the currently logged-in user
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (ModelState.IsValid)
            {
                // Assign the required values automatically on the server-side
                product.UserId = userId;
                product.Status = ProductStatus.Pending; // All new products start as Pending
                product.CreatedDate = DateTime.Now;

                _context.Add(product);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            // If model state is invalid, return the view with the product data
            return View(product);
        }

        // GET: Products/Edit/5 (Checks for ownership)
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var product = await _context.Products.FindAsync(id);
            if (product == null)
            {
                return NotFound();
            }

            // Security Check: Only the owner can edit their product
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (product.UserId != userId)
            {
                return Forbid();
            }

            // We no longer need this line
            // ViewData["UserId"] = new SelectList(_context.Users, "Id", "Id", product.UserId);
            return View(product);
        }

        // POST: Products/Edit/5 (Checks for ownership)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Title,Description,Price,Status,ImageUrl,CreatedDate")] Product product)
        {
            if (id != product.Id)
            {
                return NotFound();
            }

            // Security Check: Find the product in the DB to verify ownership
            var originalProduct = await _context.Products.AsNoTracking().FirstOrDefaultAsync(p => p.Id == id);
            if (originalProduct == null || originalProduct.UserId != User.FindFirstValue(ClaimTypes.NameIdentifier))
            {
                return Forbid();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    // Preserve the UserId from the original product
                    product.UserId = originalProduct.UserId;

                    _context.Update(product);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ProductExists(product.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }

            return View(product);
        }

        // GET: Products/Delete/5 (Checks for ownership)
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var product = await _context.Products
                .Include(p => p.User)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (product == null)
            {
                return NotFound();
            }

            // Security Check: Only the owner can delete their product
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (product.UserId != userId)
            {
                return Forbid();
            }

            return View(product);
        }

        // POST: Products/Delete/5 (Checks for ownership)
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var product = await _context.Products.FindAsync(id);

            // Security Check: Verify ownership before deleting
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