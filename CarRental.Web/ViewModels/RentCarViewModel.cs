using System.ComponentModel.DataAnnotations;

namespace CarRental.Web.ViewModels
{
    /// <summary>
    /// ViewModel for the rental booking form on the Car Details page.
    /// Contains the date range input and the car reference.
    /// </summary>
    public class RentCarViewModel
    {
        public int CarId { get; set; }

        [Required(ErrorMessage = "Start date is required.")]
        [DataType(DataType.Date)]
        public DateTime StartDate { get; set; }

        [Required(ErrorMessage = "End date is required.")]
        [DataType(DataType.Date)]
        public DateTime EndDate { get; set; }
    }
}
