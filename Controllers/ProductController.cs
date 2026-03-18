using Microsoft.AspNetCore.Mvc;
using QuickBuy.Models; // ✅ Only one time
using QuickBuy.Services.Interfaces;
using QuickBuy.Web.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace QuickBuy.Web.Controllers
{
    public class ProductController : Controller
    {
        private readonly IProductService _productService;

        public ProductController(IProductService productService)
        {
            _productService = productService;
        }

        public async Task<IActionResult> Index(int? categoryId, string search = "")
        {
            IEnumerable<Product> products;

            if (!string.IsNullOrEmpty(search))
            {
                products = await _productService.SearchProductsAsync(search);
                ViewBag.SearchTerm = search;
            }
            else if (categoryId.HasValue)
            {
                products = await _productService.GetProductsByCategoryAsync(categoryId.Value);
                ViewBag.CategoryId = categoryId;
            }
            else
            {
                products = await _productService.GetAllProductsAsync();
            }

            var productVMs = products.Select(p => new ProductVM
            {
                Id = p.Id,
                Name = p.Name,
                Description = p.Description,
                Price = p.Price,
                DiscountPrice = p.DiscountPrice,
                StockQuantity = p.StockQuantity,
                ImageUrl = p.ImageUrl,
                CategoryId = p.CategoryId,
                CategoryName = p.Category?.Name,
                IsActive = p.IsActive
            }).ToList();

            return View(productVMs);
        }

        public async Task<IActionResult> Details(int id)
        {
            var product = await _productService.GetProductByIdAsync(id);
            if (product == null)
            {
                TempData["Error"] = "Product not found";
                return RedirectToAction("Index");
            }

            var productVM = new ProductVM
            {
                Id = product.Id,
                Name = product.Name,
                Description = product.Description,
                Price = product.Price,
                DiscountPrice = product.DiscountPrice,
                StockQuantity = product.StockQuantity,
                ImageUrl = product.ImageUrl ?? "/images/products/default.jpg",
                CategoryId = product.CategoryId,
                CategoryName = product.Category?.Name ?? "Uncategorized",
                IsActive = product.IsActive
            };

            // Get related products safely
            var relatedProducts = await _productService.GetProductsByCategoryAsync(product.CategoryId);
            ViewBag.RelatedProducts = relatedProducts?
                .Where(p => p.Id != id)
                .Take(4)
                .Select(p => new ProductVM
                {
                    Id = p.Id,
                    Name = p.Name,
                    Price = p.Price,
                    DiscountPrice = p.DiscountPrice,
                    ImageUrl = p.ImageUrl ?? "/images/products/default.jpg"
                }) ?? new List<ProductVM>();

            return View(productVM);
        }

        [HttpPost]
        public async Task<IActionResult> Search(string searchTerm)
        {
            if (string.IsNullOrWhiteSpace(searchTerm))
            {
                return RedirectToAction("Index");
            }

            return RedirectToAction("Index", new { search = searchTerm });
        }

        public async Task<IActionResult> Category(int id)
        {
            return RedirectToAction("Index", new { categoryId = id });
        }
    }
}