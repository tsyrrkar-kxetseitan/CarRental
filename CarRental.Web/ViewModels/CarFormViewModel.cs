using System.ComponentModel.DataAnnotations;

namespace CarRental.Web.ViewModels
{
    /// <summary>
    /// ViewModel for the car admin form — Add/Edit.
    /// </summary>
    public class CarFormViewModel
    {
        public int Id { get; set; }

        [Required]
        [MaxLength(50)]
        public string Brand { get; set; } = string.Empty;

        [Required]
        [MaxLength(50)]
        public string Model { get; set; } = string.Empty;

        [Required]
        [Range(0.01, 99999.99)]
        [Display(Name = "Price Per Day (€)")]
        public decimal PricePerDay { get; set; }

        [MaxLength(500)]
        public string? Description { get; set; }

        [MaxLength(300)]
        [Display(Name = "Image URL")]
        public string? ImageUrl { get; set; }

        [Range(2, 9)]
        public int Seats { get; set; } = 5;

        [Required]
        [MaxLength(100)]
        public string Location { get; set; } = string.Empty;

        [Required]
        [MaxLength(20)]
        [Display(Name = "License Plate")]
        public string LicensePlate { get; set; } = string.Empty;

        // ─── Extended Specifications ──────────────────────────────

        [MaxLength(50)]
        [Display(Name = "Chassis / Body Type")]
        public string? Chassis { get; set; }

        [MaxLength(50)]
        [Display(Name = "Fuel Consumption")]
        public string? FuelConsumption { get; set; }

        [Range(1990, 2030)]
        [Display(Name = "Fabrication Year")]
        public int? FabricationYear { get; set; }

        [MaxLength(100)]
        [Display(Name = "Engine Type")]
        public string? EngineType { get; set; }

        [MaxLength(30)]
        public string? Transmission { get; set; }

        [Range(50, 2000)]
        [Display(Name = "Horsepower (HP)")]
        public int? Horsepower { get; set; }
    }
}
