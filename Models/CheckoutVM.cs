using System.ComponentModel.DataAnnotations;

namespace QuickBuy.Web.Models
{
    public class CheckoutVM
    {
        [Required(ErrorMessage = "Full name is required")]
        [StringLength(100)]
        public string FullName { get; set; }

        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Invalid email address")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Phone number is required")]
        [Phone(ErrorMessage = "Invalid phone number")]
        public string PhoneNumber { get; set; }

        [Required(ErrorMessage = "Address is required")]
        [StringLength(200)]
        public string Address { get; set; }

        [Required(ErrorMessage = "City is required")]
        [StringLength(50)]
        public string City { get; set; }

        [Required(ErrorMessage = "State is required")]
        [StringLength(50)]
        public string State { get; set; }

        [Required(ErrorMessage = "Zip code is required")]
        [StringLength(10)]
        public string ZipCode { get; set; }

        public string OrderNotes { get; set; }

        // Payment information
        [Required(ErrorMessage = "Card holder name is required")]
        public string CardHolderName { get; set; }

        [Required(ErrorMessage = "Card number is required")]
        [CreditCard(ErrorMessage = "Invalid credit card number")]
        public string CardNumber { get; set; }

        [Required(ErrorMessage = "Expiry date is required")]
        [RegularExpression(@"^(0[1-9]|1[0-2])\/?([0-9]{2})$",
            ErrorMessage = "Invalid expiry date format (MM/YY)")]
        public string ExpiryDate { get; set; }

        [Required(ErrorMessage = "CVV is required")]
        [StringLength(4, MinimumLength = 3, ErrorMessage = "CVV must be 3-4 digits")]
        public string CVV { get; set; }

        // Cart summary
        public decimal Subtotal { get; set; }
        public decimal Tax { get; set; }
        public decimal Shipping { get; set; }
        public decimal Total { get; set; }
    }
}