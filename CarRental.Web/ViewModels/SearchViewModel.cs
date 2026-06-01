namespace CarRental.Web.ViewModels
{
    /// <summary>
    /// ViewModel for the Welcome page search wizard and Browse Cars filter.
    /// </summary>
    public class SearchViewModel
    {
        public string? Location { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string? Brand { get; set; }
        public string? Model { get; set; }
        public int? Seats { get; set; }
        /// <summary>
        /// Free-text search query (e.g. "BMW X5" or "Audi").
        /// </summary>
        public string? Query { get; set; }
    }
}
