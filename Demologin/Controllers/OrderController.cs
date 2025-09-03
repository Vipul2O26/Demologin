using Demologin.Data;
using Demologin.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Demologin.Controllers
{
    [Authorize]
    public class OrderController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public OrderController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // ✅ Direct Buy (from Product page)
        [HttpPost]
        public async Task<IActionResult> BuyNow(Guid productId, int quantity = 1)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Unauthorized();

            var product = await _context.Products.FindAsync(productId);
            if (product == null) return NotFound();

            var order = new Order
            {
                ProductId = product.Id,
                UserId = user.Id,
                Quantity = quantity,
                TotalAmount = product.Price * quantity,
                OrderDate = DateTime.UtcNow,
                Status = "Pending"
            };

            _context.Orders.Add(order);
            await _context.SaveChangesAsync();

            return RedirectToAction("Checkout", new { orderId = order.Id });
        }

        // ✅ Checkout Page (for BuyNow or Cart)
        [HttpGet]
        public async Task<IActionResult> Checkout(Guid? orderId = null)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Unauthorized();

            if (orderId.HasValue)
            {
                // Single order checkout
                var order = await _context.Orders
                    .Include(o => o.Product)
                    .FirstOrDefaultAsync(o => o.Id == orderId && o.UserId == user.Id);

                if (order == null) return NotFound();

                return View(new List<Order> { order });
            }
            else
            {
                // Checkout from cart
                var cartItems = await _context.Carts
                    .Include(c => c.Product)
                    .Where(c => c.UserId == user.Id)
                    .ToListAsync();

                if (!cartItems.Any())
                    return RedirectToAction("Index", "Cart");

                var orders = cartItems.Select(c => new Order
                {
                    ProductId = c.ProductId,
                    UserId = user.Id,
                    Quantity = c.Quantity,
                    TotalAmount = c.Product.Price * c.Quantity,
                    OrderDate = DateTime.UtcNow,
                    Status = "Pending"
                }).ToList();

                _context.Orders.AddRange(orders);
                _context.Carts.RemoveRange(cartItems);
                await _context.SaveChangesAsync();

                return View(orders);
            }
        }

        // ✅ Order History
        public async Task<IActionResult> MyOrders()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Unauthorized();

            var orders = await _context.Orders
                .Include(o => o.Product)
                .Where(o => o.UserId == user.Id)
                .OrderByDescending(o => o.OrderDate)
                .ToListAsync();

            return View(orders);
        }
    }
}
