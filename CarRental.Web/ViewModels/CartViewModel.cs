using CarRental.Core.Entities;
using System.ComponentModel.DataAnnotations;

namespace CarRental.Web.ViewModels
{
    /// <summary>
    /// Represents a single item in the shopping cart (a pending booking).
    /// Stored in session before payment is confirmed.
    /// </summary>
    public class CartItem
    {
        public int CarId { get; set; }
        public string CarBrand { get; set; } = string.Empty;
        public string CarModel { get; set; } = string.Empty;
        public string LicensePlate { get; set; } = string.Empty;
        public string Location { get; set; } = string.Empty;
        public decimal PricePerDay { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public int TotalDays { get; set; }
        public decimal TotalPrice { get; set; }
    }

    /// <summary>
    /// ViewModel for the Cart/Payment page.
    /// </summary>
    public class CartViewModel
    {
        public List<CartItem> Items { get; set; } = new();
        public decimal Subtotal => Items.Sum(i => i.TotalPrice);
        public decimal AccountBalance { get; set; }
        public bool UseBalance { get; set; }

        /// <summary>
        /// Amount deducted from balance (capped at Subtotal).
        /// </summary>
        public decimal BalanceDeduction => UseBalance ? Math.Min(AccountBalance, Subtotal) : 0;

        /// <summary>
        /// Final amount to be charged to card.
        /// </summary>
        public decimal AmountDue => Subtotal - BalanceDeduction;
    }

    /// <summary>
    /// Form data submitted when confirming payment.
    /// Includes validation constraints for card data.
    /// </summary>
    public class PaymentFormModel
    {
        [Required(ErrorMessage = "Card number is required.")]
        [RegularExpression(@"^\d{16}$", ErrorMessage = "Card number must be exactly 16 digits.")]
        [Display(Name = "Card Number")]
        public string CardNumber { get; set; } = string.Empty;

        [Required(ErrorMessage = "Cardholder name is required.")]
        [RegularExpression(@"^[a-zA-ZÀ-ÿ\s\-'\.]+$", ErrorMessage = "Cardholder name must contain only letters, spaces, and hyphens.")]
        [Display(Name = "Card Holder Name")]
        public string CardHolder { get; set; } = string.Empty;

        [Required(ErrorMessage = "Expiry date is required.")]
        [RegularExpression(@"^(0[1-9]|1[0-2])\/\d{2}$", ErrorMessage = "Expiry date must be in MM/YY format with a valid month (01–12).")]
        [Display(Name = "Expiry Date")]
        public string ExpiryDate { get; set; } = string.Empty;

        [Required(ErrorMessage = "CVV is required.")]
        [RegularExpression(@"^\d{3}$", ErrorMessage = "CVV must be exactly 3 digits.")]
        [Display(Name = "CVC/CVV")]
        public string Cvv { get; set; } = string.Empty;

        public bool UseBalance { get; set; }
    }
}
