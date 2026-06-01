using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CarRental.Core.Entities
{
    /// <summary>
    /// Represents a rental booking linking a User to a Car for a specific period.
    /// Status tracks the lifecycle: Active, Canceled (by client), CanceledByAdmin.
    /// CreatedAt records when the booking was originally made for audit/log purposes.
    /// </summary>
    public class RentalBooking
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int UserId { get; set; }

        [Required]
        public int CarId { get; set; }

        [Required]
        [DataType(DataType.Date)]
        public DateTime StartDate { get; set; }

        [Required]
        [DataType(DataType.Date)]
        public DateTime EndDate { get; set; }

        /// <summary>
        /// Calculated as (EndDate - StartDate).Days * Car.PricePerDay.
        /// Persisted for historical accuracy (price may change later).
        /// </summary>
        [Column(TypeName = "decimal(18,2)")]
        public decimal TotalPrice { get; set; }

        /// <summary>
        /// Booking lifecycle status:
        /// - "Active"          : booking is valid
        /// - "Canceled"        : canceled by the client
        /// - "CanceledByAdmin" : canceled by admin (car marked unavailable or deleted)
        /// </summary>
        [Required]
        [MaxLength(20)]
        public string Status { get; set; } = "Active";

        /// <summary>
        /// Timestamp of when this booking was created (for rental log audit trail).
        /// </summary>
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        /// <summary>
        /// Amount refunded when the booking was canceled.
        /// Null for active bookings. Used for revenue/expense tracking.
        /// </summary>
        [Column(TypeName = "decimal(18,2)")]
        public decimal? RefundAmount { get; set; }

        // Navigation properties — EF Core resolves these via foreign keys
        [ForeignKey(nameof(UserId))]
        public virtual User? User { get; set; }

        [ForeignKey(nameof(CarId))]
        public virtual Car? Car { get; set; }
    }
}
