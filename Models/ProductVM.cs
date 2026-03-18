using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.ComponentModel.DataAnnotations;

namespace QuickBuy.Web.Models
{
    public class ProductVM
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Product name is required")]
        [StringLength(200, ErrorMessage = "Product name cannot exceed 200 characters")]
        public string Name { get; set; }

        [StringLength(1000, ErrorMessage = "Description cannot exceed 1000 characters")]
        public string Description { get; set; }

        [Required(ErrorMessage = "Price is required")]
        [Range(0.01, 100000, ErrorMessage = "Price must be between 0.01 and 100000")]
        public decimal Price { get; set; }

        [Range(0.01, 100000, ErrorMessage = "Discount price must be between 0.01 and 100000")]
        public decimal? DiscountPrice { get; set; }

        [Required(ErrorMessage = "Stock quantity is required")]
        [Range(0, 10000, ErrorMessage = "Stock quantity must be between 0 and 10000")]
        public int StockQuantity { get; set; }

        [Url(ErrorMessage = "Please enter a valid URL")]
        public string ImageUrl { get; set; }

        [Required(ErrorMessage = "Category is required")]
        public int CategoryId { get; set; }
        public string CategoryName { get; set; }

        public bool IsActive { get; set; } = true;

        // Calculated property
        public decimal FinalPrice => DiscountPrice ?? Price;
        public decimal Savings => DiscountPrice.HasValue ? Price - DiscountPrice.Value : 0;
        public int DiscountPercentage => DiscountPrice.HasValue ?
            (int)((Price - DiscountPrice.Value) / Price * 100) : 0;
        public IEnumerable<SelectListItem> Categories { get; set; }

        public bool HasDiscount()
        {
            return DiscountPrice.HasValue && DiscountPrice.Value > 0;
        }
    }
}