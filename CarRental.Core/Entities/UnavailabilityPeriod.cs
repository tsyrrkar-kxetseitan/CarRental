using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CarRental.Core.Entities
{
    /// <summary>
    /// Represents a period during which a car is unavailable (maintenance, repairs, etc.).
    /// Set by the Admin. Overlapping client bookings are automatically canceled.
    /// </summary>
    public class UnavailabilityPeriod
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int CarId { get; set; }

        [Required]
        [DataType(DataType.Date)]
        public DateTime StartDate { get; set; }

        [Required]
        [DataType(DataType.Date)]
        public DateTime EndDate { get; set; }

        /// <summary>
        /// Optional reason for unavailability (e.g. "Maintenance", "Repair", "Inspection").
        /// </summary>
        [MaxLength(200)]
        public string? Reason { get; set; }

        /// <summary>
        /// Timestamp of when this period was created.
        /// </summary>
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        // Navigation property
        [ForeignKey(nameof(CarId))]
        public virtual Car? Car { get; set; }
    }
}
