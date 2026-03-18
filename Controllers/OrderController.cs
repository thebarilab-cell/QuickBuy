using Microsoft.AspNetCore.Mvc;
using QuickBuy.Services.Interfaces;
using QuickBuy.Web.Models;
using System.Threading.Tasks;
using QuickBuy.Models;


namespace QuickBuy.Web.Controllers
{
    public class OrderController : Controller
    {
        private readonly IOrderService _orderService;
        private readonly ICartService _cartService;

        public OrderController(IOrderService orderService, ICartService cartService)
        {
            _orderService = orderService;
            _cartService = cartService;
        }

        [HttpGet]
        public async Task<IActionResult> Checkout()
        {
            var userId = GetUserId();
            var cartTotal = await _cartService.GetCartTotalAsync(userId);

            if (cartTotal == 0)
            {
                TempData["Error"] = "Your cart is empty";
                return RedirectToAction("Index", "Cart");
            }

            var checkoutVM = new CheckoutVM
            {
                Subtotal = cartTotal,
                Tax = cartTotal * 0.1m, // 10% tax
                Shipping = cartTotal > 50 ? 0 : 5.99m, // Free shipping over $50
                Total = cartTotal + (cartTotal * 0.1m) + (cartTotal > 50 ? 0 : 5.99m)
            };

            return View(checkoutVM);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Checkout(CheckoutVM model)
        {
            if (!ModelState.IsValid)
            {
                // Recalculate totals
                model.Subtotal = await _cartService.GetCartTotalAsync(GetUserId());
                model.Tax = model.Subtotal * 0.1m;
                model.Shipping = model.Subtotal > 50 ? 0 : 5.99m;
                model.Total = model.Subtotal + model.Tax + model.Shipping;

                return View(model);
            }

            var userId = GetUserId();
            var shippingAddress = $"{model.Address}, {model.City}, {model.State} {model.ZipCode}";

            // Create order
            var order = await _orderService.CreateOrderAsync(userId, shippingAddress);

            if (order == null)
            {
                TempData["Error"] = "Failed to create order. Please try again.";
                return RedirectToAction("Checkout");
            }

            // Process payment (simulated)
            // In a real application, integrate with payment gateway here
            await _orderService.UpdateOrderStatusAsync(order.Id, "Paid");

            // Send confirmation email (simulated)
            // await SendOrderConfirmationEmail(order.Id);

            TempData["OrderNumber"] = order.OrderNumber;
            TempData["OrderTotal"] = order.TotalAmount.ToString("C");

            return RedirectToAction("Success", new { orderId = order.Id });
        }

        public async Task<IActionResult> Success(int orderId)
        {
            var order = await _orderService.GetOrderByIdAsync(orderId);
            if (order == null)
            {
                return NotFound();
            }

            return View(order);
        }

        public async Task<IActionResult> MyOrders()
        {
            var userId = GetUserId();
            var orders = await _orderService.GetOrdersByUserIdAsync(userId);
            return View(orders);
        }

        public async Task<IActionResult> OrderDetails(int id)
        {
            var order = await _orderService.GetOrderByIdAsync(id);
            if (order == null || order.UserId != GetUserId())
            {
                return NotFound();
            }

            return View(order);
        }

        [HttpPost]
        public async Task<IActionResult> CancelOrder(int id)
        {
            var success = await _orderService.CancelOrderAsync(id);

            if (success)
            {
                TempData["Success"] = "Order cancelled successfully";
            }
            else
            {
                TempData["Error"] = "Failed to cancel order. Order may already be shipped.";
            }

            return RedirectToAction("MyOrders");
        }

        private int GetUserId()
        {
            // This is a placeholder. Implement proper user authentication
            return HttpContext.Session.GetInt32("UserId") ?? 1;
        }
    }
}