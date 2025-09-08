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

            // ✅ Stock + Threshold check
            if (quantity > product.Stock)
            {
                TempData["Error"] = "Not enough stock available.";
                return RedirectToAction("Details", "Products", new { id = product.Id });
            }

            if (product.Stock - quantity < product.Threshold)
            {
                TempData["Error"] = $"Cannot order. Stock would fall below threshold ({product.Threshold}).";
                return RedirectToAction("Details", "Products", new { id = product.Id });
            }

            // ✅ Create order
            var order = new Order
            {
                ProductId = product.Id,
                UserId = user.Id,
                Quantity = quantity,
                TotalAmount = product.Price * quantity,
                OrderDate = DateTime.UtcNow,
                Status = "Pending"
            };

            // Deduct stock
            product.Stock -= quantity;

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

                // ✅ Validate stock for each item
                foreach (var item in cartItems)
                {
                    if (item.Quantity > item.Product.Stock)
                    {
                        TempData["Error"] = $"Not enough stock for {item.Product.Title}.";
                        return RedirectToAction("Index", "Cart");
                    }

                    if (item.Product.Stock - item.Quantity < item.Product.Threshold)
                    {
                        TempData["Error"] = $"Cannot order {item.Product.Title}. Stock would fall below threshold.";
                        return RedirectToAction("Index", "Cart");
                    }
                }

                // ✅ All valid → create orders & deduct stock
                var orders = new List<Order>();
                foreach (var item in cartItems)
                {
                    var order = new Order
                    {
                        ProductId = item.ProductId,
                        UserId = user.Id,
                        Quantity = item.Quantity,
                        TotalAmount = item.Product.Price * item.Quantity,
                        OrderDate = DateTime.UtcNow,
                        Status = "Pending"
                    };

                    item.Product.Stock -= item.Quantity; // Deduct stock
                    orders.Add(order);
                }

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

        // ✅ Cancel Order (restore stock)
        [HttpPost]
        public async Task<IActionResult> Cancel(Guid orderId)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Unauthorized();

            var order = await _context.Orders
                .Include(o => o.Product)
                .FirstOrDefaultAsync(o => o.Id == orderId && o.UserId == user.Id);

            if (order == null) return NotFound();

            if (order.Status != "Pending")
            {
                TempData["Error"] = "Only pending orders can be cancelled.";
                return RedirectToAction("MyOrders");
            }

            // ✅ Restore stock
            order.Product.Stock += order.Quantity;
            order.Status = "Cancelled";

            await _context.SaveChangesAsync();

            TempData["Success"] = "Your order has been cancelled and stock restored.";
            return RedirectToAction("MyOrders");
        }
    }
}
