using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CarRental.Core.Entities
{
    /// <summary>
    /// Represents an in-site notification for a user.
    /// Used for booking confirmations, cancellation alerts, and rental reminders.
    /// </summary>
    public class Notification
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int UserId { get; set; }

        [Required]
        [MaxLength(100)]
        public string Title { get; set; } = string.Empty;

        [Required]
        [MaxLength(1000)]
        public string Message { get; set; } = string.Empty;

        /// <summary>
        /// Type of notification: "Booking", "Cancellation", "Reminder", "Welcome"
        /// </summary>
        [Required]
        [MaxLength(30)]
        public string Type { get; set; } = "Booking";

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public bool IsRead { get; set; } = false;

        [ForeignKey(nameof(UserId))]
        public virtual User? User { get; set; }
    }
}
