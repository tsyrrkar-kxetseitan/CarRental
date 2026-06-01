using System.ComponentModel.DataAnnotations;

namespace CarRental.Web.ViewModels
{
    /// <summary>
    /// ViewModel for the login form — keeps UI concerns separate from the domain entity.
    /// OOP Principle: Separation of Concerns — ViewModels belong to the UI layer.
    /// </summary>
    public class LoginViewModel
    {
        [Required(ErrorMessage = "Username is required.")]
        [StringLength(50, MinimumLength = 3)]
        public string Username { get; set; } = string.Empty;

        [Required(ErrorMessage = "Password is required.")]
        [DataType(DataType.Password)]
        public string Password { get; set; } = string.Empty;
    }
}
