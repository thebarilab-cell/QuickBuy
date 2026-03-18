using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuickBuy.Data;
using QuickBuy.Models;
using QuickBuy.Services.Interfaces;
using System.Linq;
using System.Threading.Tasks;

namespace QuickBuy.Web.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IProductService _productService;
        private readonly IOrderService _orderService;

        public AdminController(
            ApplicationDbContext context,
            IProductService productService,
            IOrderService orderService)
        {
            _context = context;
            _productService = productService;
            _orderService = orderService;
        }

        public IActionResult Dashboard()
        {
            var totalProducts = _context.Products.Count(p => p.IsActive);
            var totalOrders = _context.Orders.Count();
            var totalUsers = _context.Users.Count(u => u.IsActive);
            var totalRevenue = _context.Orders
                .Where(o => o.PaymentStatus == "Paid")
                .Sum(o => o.TotalAmount);

            ViewBag.TotalProducts = totalProducts;
            ViewBag.TotalOrders = totalOrders;
            ViewBag.TotalUsers = totalUsers;
            ViewBag.TotalRevenue = totalRevenue.ToString("C");

            // Recent orders
            var recentOrders = _context.Orders
                .Include(o => o.User)
                .OrderByDescending(o => o.OrderDate)
                .Take(10)
                .ToList();

            ViewBag.RecentOrders = recentOrders;

            return View();
        }

        public async Task<IActionResult> Products(string search = "", int page = 1, int pageSize = 10)
        {
            IQueryable<Product> query = _context.Products
                .Include(p => p.Category)
                .Where(p => p.IsActive);

            if (!string.IsNullOrEmpty(search))
            {
                query = query.Where(p =>
                    p.Name.Contains(search) ||
                    p.Description.Contains(search) ||
                    p.Category.Name.Contains(search));
            }

            var totalItems = await query.CountAsync();
            var totalPages = (int)System.Math.Ceiling(totalItems / (double)pageSize);

            var products = await query
                .OrderByDescending(p => p.CreatedDate)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            ViewBag.SearchTerm = search;
            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = totalPages;
            ViewBag.PageSize = pageSize;

            return View(products);
        }

        [HttpGet]
        public IActionResult AddProduct()
        {
            ViewBag.Categories = _context.Categories.Where(c => c.IsActive).ToList();
            return View(new Product());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddProduct(Product product)
        {
            if (ModelState.IsValid)
            {
                product.CreatedDate = System.DateTime.Now;
                product.IsActive = true;

                _context.Products.Add(product);
                await _context.SaveChangesAsync();

                TempData["Success"] = "Product added successfully!";
                return RedirectToAction("Products");
            }

            ViewBag.Categories = _context.Categories.Where(c => c.IsActive).ToList();
            return View(product);
        }

        [HttpGet]
        public async Task<IActionResult> EditProduct(int id)
        {
            var product = await _context.Products
                .Include(p => p.Category)
                .FirstOrDefaultAsync(p => p.Id == id && p.IsActive);

            if (product == null)
            {
                return NotFound();
            }

            ViewBag.Categories = _context.Categories.Where(c => c.IsActive).ToList();
            return View(product);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditProduct(Product product)
        {
            if (ModelState.IsValid)
            {
                product.UpdatedDate = System.DateTime.Now;
                _context.Products.Update(product);
                await _context.SaveChangesAsync();

                TempData["Success"] = "Product updated successfully!";
                return RedirectToAction("Products");
            }

            ViewBag.Categories = _context.Categories.Where(c => c.IsActive).ToList();
            return View(product);
        }

        [HttpPost]
        public async Task<IActionResult> DeleteProduct(int id)
        {
            var product = await _context.Products.FindAsync(id);
            if (product == null)
            {
                return NotFound();
            }

            product.IsActive = false;
            product.UpdatedDate = System.DateTime.Now;
            await _context.SaveChangesAsync();

            TempData["Success"] = "Product deleted successfully!";
            return RedirectToAction("Products");
        }

        public async Task<IActionResult> Categories()
        {
            var categories = await _context.Categories
                .Where(c => c.IsActive)
                .ToListAsync();

            return View(categories);
        }

        [HttpPost]
        public async Task<IActionResult> AddCategory(Category category)
        {
            if (ModelState.IsValid)
            {
                category.IsActive = true;
                _context.Categories.Add(category);
                await _context.SaveChangesAsync();

                TempData["Success"] = "Category added successfully!";
            }

            return RedirectToAction("Categories");
        }

        [HttpPost]
        public async Task<IActionResult> DeleteCategory(int id)
        {
            var category = await _context.Categories.FindAsync(id);
            if (category == null)
            {
                return NotFound();
            }

            // Check if category has products
            var hasProducts = await _context.Products.AnyAsync(p => p.CategoryId == id && p.IsActive);
            if (hasProducts)
            {
                TempData["Error"] = "Cannot delete category with active products";
                return RedirectToAction("Categories");
            }

            category.IsActive = false;
            await _context.SaveChangesAsync();

            TempData["Success"] = "Category deleted successfully!";
            return RedirectToAction("Categories");
        }

        public async Task<IActionResult> Orders(string status = "", int page = 1, int pageSize = 20)
        {
            IQueryable<Order> query = _context.Orders
                .Include(o => o.User)
                .Include(o => o.OrderItems)
                .OrderByDescending(o => o.OrderDate);

            if (!string.IsNullOrEmpty(status))
            {
                query = query.Where(o => o.OrderStatus == status);
            }

            var totalItems = await query.CountAsync();
            var totalPages = (int)System.Math.Ceiling(totalItems / (double)pageSize);

            var orders = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            ViewBag.Status = status;
            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = totalPages;
            ViewBag.PageSize = pageSize;

            return View(orders);
        }

        [HttpPost]
        public async Task<IActionResult> UpdateOrderStatus(int orderId, string status)
        {
            var success = await _orderService.UpdateOrderStatusAsync(orderId, status);

            if (success)
            {
                TempData["Success"] = $"Order status updated to {status}";
            }
            else
            {
                TempData["Error"] = "Failed to update order status";
            }

            return RedirectToAction("Orders");
        }

        public async Task<IActionResult> OrderDetails(int id)
        {
            var order = await _context.Orders
                .Include(o => o.User)
                .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Product)
                .FirstOrDefaultAsync(o => o.Id == id);

            if (order == null)
            {
                return NotFound();
            }

            return View(order);
        }

        public async Task<IActionResult> Users()
        {
            var users = await _context.Users
                .Where(u => u.IsActive)
                .OrderByDescending(u => u.CreatedDate)
                .ToListAsync();

            return View(users);
        }

        [HttpPost]
        public async Task<IActionResult> ToggleUserStatus(int id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            user.IsActive = !user.IsActive;
            await _context.SaveChangesAsync();

            TempData["Success"] = $"User {(user.IsActive ? "activated" : "deactivated")} successfully!";
            return RedirectToAction("Users");
        }

        [HttpPost]
        public async Task<IActionResult> ChangeUserRole(int id, string role)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            user.Role = role;
            await _context.SaveChangesAsync();

            TempData["Success"] = $"User role changed to {role}";
            return RedirectToAction("Users");
        }

        public IActionResult Reports()
        {
            // Sales by month
            var monthlySales = _context.Orders
                .Where(o => o.PaymentStatus == "Paid")
                .GroupBy(o => new { o.OrderDate.Year, o.OrderDate.Month })
                .Select(g => new
                {
                    Year = g.Key.Year,
                    Month = g.Key.Month,
                    Total = g.Sum(o => o.TotalAmount),
                    Count = g.Count()
                })
                .OrderByDescending(x => x.Year)
                .ThenByDescending(x => x.Month)
                .Take(12)
                .ToList();

            // Top selling products
            var topProducts = _context.OrderItems
                .Include(oi => oi.Product)
                .GroupBy(oi => oi.ProductId)
                .Select(g => new
                {
                    ProductId = g.Key,
                    ProductName = g.First().Product.Name,
                    TotalQuantity = g.Sum(oi => oi.Quantity),
                    TotalRevenue = g.Sum(oi => oi.Quantity * oi.UnitPrice)
                })
                .OrderByDescending(x => x.TotalQuantity)
                .Take(10)
                .ToList();

            ViewBag.MonthlySales = monthlySales;
            ViewBag.TopProducts = topProducts;

            return View();
        }
    }
}