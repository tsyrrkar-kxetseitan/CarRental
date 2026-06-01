using CarRental.Core.Entities;

namespace CarRental.Web.ViewModels
{
    /// <summary>
    /// ViewModel for the Account/MyAccount page, containing user info,
    /// account balance, and full rental history.
    /// </summary>
    public class AccountViewModel
    {
        public int UserId { get; set; }
        public string Username { get; set; } = string.Empty;
        public string Role { get; set; } = "Client";
        public decimal AccountBalance { get; set; }
        public List<RentalBooking> RentalHistory { get; set; } = new();
    }
}
