using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CarRental.Core.Entities
{
    /// <summary>
    /// Represents a car available for rental.
    /// IsDeleted enables soft-delete so rental history is preserved in logs.
    /// Availability is now date-range based via UnavailabilityPeriod entries.
    /// </summary>
    public class Car
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(50)]
        public string Brand { get; set; } = string.Empty;

        [Required]
        [MaxLength(50)]
        public string Model { get; set; } = string.Empty;

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        [Range(0.01, 99999.99)]
        public decimal PricePerDay { get; set; }

        /// <summary>
        /// Soft-delete flag. When true, the car is hidden from browsing
        /// but its rental history is preserved in the rental log.
        /// </summary>
        public bool IsDeleted { get; set; } = false;

        [MaxLength(500)]
        public string? Description { get; set; }

        [MaxLength(300)]
        public string? ImageUrl { get; set; }

        /// <summary>
        /// Number of passenger seats (e.g. 2, 5, 7, 8).
        /// </summary>
        public int Seats { get; set; } = 5;

        /// <summary>
        /// City where this car is located (e.g. "Cluj-Napoca", "București").
        /// </summary>
        [Required]
        [MaxLength(100)]
        public string Location { get; set; } = string.Empty;

        /// <summary>
        /// Romanian license plate in standard format (e.g. "CJ04ABC", "B123XYZ").
        /// Must be unique per car.
        /// </summary>
        [Required]
        [MaxLength(20)]
        public string LicensePlate { get; set; } = string.Empty;

        // ─── Extended Specifications ───────────────────────────────────────

        /// <summary>
        /// Body type / chassis (e.g. "Sedan", "SUV", "Hatchback", "Coupe", "Estate").
        /// </summary>
        [MaxLength(50)]
        public string? Chassis { get; set; }

        /// <summary>
        /// Average fuel consumption (e.g. "6.5 L/100km", "18 kWh/100km").
        /// </summary>
        [MaxLength(50)]
        public string? FuelConsumption { get; set; }

        /// <summary>
        /// Year the car was manufactured (e.g. 2023).
        /// </summary>
        public int? FabricationYear { get; set; }

        /// <summary>
        /// Engine type and displacement (e.g. "2.0L TDI Diesel", "1.5L TSI Petrol").
        /// </summary>
        [MaxLength(100)]
        public string? EngineType { get; set; }

        /// <summary>
        /// Transmission type (e.g. "Automatic", "Manual", "DCT").
        /// </summary>
        [MaxLength(30)]
        public string? Transmission { get; set; }

        /// <summary>
        /// Engine power in horsepower (HP).
        /// </summary>
        public int? Horsepower { get; set; }

        // Navigation properties
        public virtual ICollection<RentalBooking> RentalBookings { get; set; } = new List<RentalBooking>();
        public virtual ICollection<UnavailabilityPeriod> UnavailabilityPeriods { get; set; } = new List<UnavailabilityPeriod>();
    }
}
