using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Demologin.Data;
using Demologin.Models;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace Demologin.Controllers
{
    [Authorize]
    public class CartController : Controller
    {
        private readonly ApplicationDbContext _context;

        public CartController(ApplicationDbContext context)
        {
            _context = context;
        }

        // Show Cart Items
        public async Task<IActionResult> Index()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var items = await _context.Carts
                .Include(c => c.Product)
                .Where(c => c.UserId == userId)
                .ToListAsync();

            return View(items);
        }
        [HttpPost]
        public async Task<IActionResult> Add(Guid productId, int quantity = 1)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var product = await _context.Products.FindAsync(productId);
            if (product == null) return NotFound();

            // ✅ Stock + Threshold check
            if (quantity > product.Stock)
            {
                TempData["Error"] = "Not enough stock available.";
                return RedirectToAction("Index", "Products");
            }

            if (product.Stock - quantity < product.Threshold)
            {
                TempData["Error"] = $"Cannot add to cart. Stock would fall below threshold ({product.Threshold}).";
                return RedirectToAction("Index", "Products");
            }

            var cartItem = new Cart
            {
                UserId = userId,
                ProductId = productId,
                Quantity = quantity
            };

            _context.Carts.Add(cartItem);
            await _context.SaveChangesAsync();

            return RedirectToAction("Index");
        }


        // Remove from Cart
        public async Task<IActionResult> Remove(Guid id)
        {
            var cartItem = await _context.Carts.FindAsync(id);
            if (cartItem != null)
            {
                _context.Carts.Remove(cartItem);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction("Index");
        }
    }
}
