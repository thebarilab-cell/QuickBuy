using Microsoft.AspNetCore.Mvc;
using QuickBuy.Services.Interfaces;
using System.Threading.Tasks;
using QuickBuy.Models;


namespace QuickBuy.Web.Controllers
{
    public class HomeController : Controller
    {
        private readonly IProductService _productService;

        public HomeController(IProductService productService)
        {
            _productService = productService;
        }

        public async Task<IActionResult> Index()
        {
            var featuredProducts = await _productService.GetFeaturedProductsAsync();
            var allProducts = await _productService.GetAllProductsAsync();

            ViewBag.FeaturedProducts = featuredProducts;
            ViewBag.AllProducts = allProducts;

            return View();
        }

        public IActionResult About()
        {
            ViewData["Title"] = "About Us";
            return View();
        }

        public IActionResult Contact()
        {
            ViewData["Title"] = "Contact Us";
            return View();
        }

        [HttpPost]
        public IActionResult Contact(string name, string email, string message)
        {
            // Handle contact form submission
            ViewData["SuccessMessage"] = "Thank you for contacting us! We'll get back to you soon.";
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        public IActionResult Terms()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View();
        }
    }
}