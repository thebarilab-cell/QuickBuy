using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuickBuy.Data;
using QuickBuy.Services.Interfaces;
using QuickBuy.Web.Models;
using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace QuickBuy.Web.Controllers
{
    public class CartController : Controller
    {
        private readonly ICartService _cartService;
        private readonly IProductService _productService;
        private readonly ApplicationDbContext _context; // ✅ Added field

        public CartController(
            ICartService cartService,
            IProductService productService,
            ApplicationDbContext context) // ✅ Added parameter
        {
            _cartService = cartService;
            _productService = productService;
            _context = context; // ✅ Initialize field
        }

        public async Task<IActionResult> Index()
        {
            var userId = await GetOrCreateUserIdAsync();
            var cart = await _cartService.GetCartByUserIdAsync(userId);

            var cartVM = new CartVM
            {
                Id = cart?.Id ?? 0,
                UserId = userId
            };

            if (cart != null && cart.CartItems.Any())
            {
                foreach (var item in cart.CartItems)
                {
                    var product = await _productService.GetProductByIdAsync(item.ProductId);
                    if (product != null)
                    {
                        cartVM.Items.Add(new CartItemVM
                        {
                            Id = item.Id,
                            ProductId = product.Id,
                            ProductName = product.Name,
                            ProductImage = product.ImageUrl,
                            Price = product.Price,
                            DiscountPrice = product.DiscountPrice,
                            Quantity = item.Quantity,
                            StockQuantity = product.StockQuantity
                        });
                    }
                }
            }

            cartVM.Subtotal = cartVM.Items.Sum(i => i.TotalPrice);
            cartVM.Tax = cartVM.Subtotal * 0.1m; // 10% tax
            cartVM.Shipping = cartVM.Subtotal > 50 ? 0 : 5.99m; // Free shipping over $50
            cartVM.Total = cartVM.Subtotal + cartVM.Tax + cartVM.Shipping;

            return View(cartVM);
        }

        [HttpPost]
        public async Task<IActionResult> AddToCart(int productId, int quantity = 1)
        {
            var userId = await GetOrCreateUserIdAsync();
            var product = await _productService.GetProductByIdAsync(productId);

            if (product == null || product.StockQuantity < quantity)
            {
                TempData["Error"] = "Product not available or insufficient stock";
                return RedirectToAction("Details", "Product", new { id = productId });
            }

            try
            {
                await _cartService.AddToCartAsync(userId, productId, quantity);
                TempData["Success"] = "Product added to cart successfully!";
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Error adding to cart: {ex.Message}";
            }

            return RedirectToAction("Details", "Product", new { id = productId });
        }

        [HttpPost]
        public async Task<IActionResult> UpdateQuantity(int cartItemId, int quantity)
        {
            var userId = await GetOrCreateUserIdAsync();
            var success = await _cartService.UpdateCartItemQuantityAsync(userId, cartItemId, quantity);

            if (success)
            {
                TempData["Success"] = "Cart updated successfully!";
            }
            else
            {
                TempData["Error"] = "Failed to update cart item";
            }

            return RedirectToAction("Index");
        }

        [HttpPost]
        public async Task<IActionResult> RemoveItem(int cartItemId)
        {
            var userId = await GetOrCreateUserIdAsync();
            var success = await _cartService.RemoveFromCartAsync(userId, cartItemId);

            if (success)
            {
                TempData["Success"] = "Item removed from cart";
            }
            else
            {
                TempData["Error"] = "Failed to remove item";
            }

            return RedirectToAction("Index");
        }

        [HttpPost]
        public async Task<IActionResult> ClearCart()
        {
            var userId = await GetOrCreateUserIdAsync();
            var success = await _cartService.ClearCartAsync(userId);

            if (success)
            {
                TempData["Success"] = "Cart cleared successfully";
            }
            else
            {
                TempData["Error"] = "Failed to clear cart";
            }

            return RedirectToAction("Index");
        }

        public async Task<IActionResult> GetCartCount()
        {
            var userId = await GetOrCreateUserIdAsync();
            var count = await _cartService.GetCartItemCountAsync(userId);
            return Json(new { count });
        }

        public async Task<IActionResult> GetCartSummary()
        {
            var userId = await GetOrCreateUserIdAsync();
            var cart = await _cartService.GetCartByUserIdAsync(userId);
            var total = await _cartService.GetCartTotalAsync(userId);

            return Json(new
            {
                itemCount = cart?.CartItems?.Sum(ci => ci.Quantity) ?? 0,
                total = total.ToString("C")
            });
        }

        private async Task<int> GetOrCreateUserIdAsync()
        {
            // 1. First check if user is authenticated
            if (User.Identity.IsAuthenticated)
            {
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (int.TryParse(userIdClaim, out int userId))
                {
                    // Verify user exists in database
                    var userExists = await _context.Users.AnyAsync(u => u.Id == userId);
                    if (userExists)
                    {
                        HttpContext.Session.SetInt32("UserId", userId);
                        return userId;
                    }
                }
            }

            // 2. Check session
            var sessionUserId = HttpContext.Session.GetInt32("UserId");
            if (sessionUserId.HasValue)
            {
                var userExists = await _context.Users.AnyAsync(u => u.Id == sessionUserId.Value);
                if (userExists)
                {
                    return sessionUserId.Value;
                }
            }

            // 3. Create a new guest user
            return await CreateGuestUserAsync();
        }

        private async Task<int> CreateGuestUserAsync()
        {
            try
            {
                // Check if a guest user already exists in session
                var guestEmail = $"guest_{Guid.NewGuid()}@quickbuy.com";

                var guestUser = new QuickBuy.Models.User // ✅ Full namespace
                {
                    FullName = "Guest User",
                    Email = guestEmail,
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword("Guest@123"),
                    PhoneNumber = "0000000000",
                    Role = "Customer",
                    CreatedDate = DateTime.Now,
                    IsActive = true
                };

                _context.Users.Add(guestUser);
                await _context.SaveChangesAsync();

                // Also create a cart for this user
                var cart = new QuickBuy.Models.Cart // ✅ Full namespace
                {
                    UserId = guestUser.Id,
                    CreatedDate = DateTime.Now
                };
                _context.Carts.Add(cart);
                await _context.SaveChangesAsync();

                // Store in session
                HttpContext.Session.SetInt32("UserId", guestUser.Id);
                HttpContext.Session.SetString("GuestUser", "true");

                return guestUser.Id;
            }
            catch (Exception ex)
            {
                // Fallback: Use admin user if guest creation fails
                var adminUser = await _context.Users
                    .FirstOrDefaultAsync(u => u.Email == "admin@quickbuy.com");

                if (adminUser != null)
                {
                    HttpContext.Session.SetInt32("UserId", adminUser.Id);
                    return adminUser.Id;
                }

                // Ultimate fallback: Use ID 1
                return 1;
            }
        }

        // Helper method for backward compatibility
        private int GetUserId()
        {
            return HttpContext.Session.GetInt32("UserId") ?? 1;
        }
    }
}