using System.Collections.Generic;

namespace QuickBuy.Web.Models
{
    public class CartVM
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public List<CartItemVM> Items { get; set; } = new List<CartItemVM>();
        public decimal Subtotal { get; set; }
        public decimal Tax { get; set; }
        public decimal Shipping { get; set; }
        public decimal Total { get; set; }

        // For cart summary
        public int TotalItems => Items.Sum(i => i.Quantity);
        public bool IsEmpty => !Items.Any();
    }

    public class CartItemVM
    {
        public int Id { get; set; }
        public int ProductId { get; set; }
        public string ProductName { get; set; }
        public string ProductImage { get; set; }
        public decimal Price { get; set; }
        public decimal? DiscountPrice { get; set; }
        public int Quantity { get; set; }
        public int StockQuantity { get; set; }

        // Calculated properties
        public decimal UnitPrice => DiscountPrice ?? Price;
        public decimal TotalPrice => UnitPrice * Quantity;
        public decimal Savings => (Price - UnitPrice) * Quantity;
        public bool IsDiscount => DiscountPrice.HasValue;
    }
}