using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CarRental.Core.Entities
{
    /// <summary>
    /// Represents an application user with authentication and role information.
    /// OOP Principle: Encapsulation — properties expose controlled access to user data.
    /// Data Annotations ensure database integrity constraints (NOT NULL, MaxLength).
    /// </summary>
    public class User
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(50)]
        public string Username { get; set; } = string.Empty;

        [Required]
        [MaxLength(256)]
        public string PasswordHash { get; set; } = string.Empty;

        /// <summary>
        /// Role must be either "Admin" or "Client".
        /// Database column is constrained to 10 characters max.
        /// </summary>
        [Required]
        [MaxLength(10)]
        public string Role { get; set; } = "Client";

        /// <summary>
        /// Account balance in EUR. New clients receive a €20 welcome bonus.
        /// Refunds from cancellations are credited here.
        /// Can be used during checkout to reduce the total.
        /// </summary>
        [Column(TypeName = "decimal(18,2)")]
        public decimal AccountBalance { get; set; } = 20.00m;

        /// <summary>
        /// User's email address for notifications (booking confirmations,
        /// cancellation alerts, rental reminders).
        /// </summary>
        [Required]
        [MaxLength(150)]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        // Navigation property — one User can have many RentalBookings (1:N relationship)
        public virtual ICollection<RentalBooking> RentalBookings { get; set; } = new List<RentalBooking>();
    }
}
