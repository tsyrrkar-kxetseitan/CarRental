using CarRental.Core.Entities;
using CarRental.Data;
using CarRental.Web.Services;
using CarRental.Web.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CarRental.Web.Controllers
{
    /// <summary>
    /// Admin Dashboard — CRUD for Cars, Set Unavailability, Delete (soft), Rental Log.
    /// Admin cancellations refund 120% of the booking value to the client's balance.
    /// </summary>
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IEmailService _emailService;
        private readonly INotificationService _notificationService;

        public AdminController(ApplicationDbContext context, IEmailService emailService, INotificationService notificationService)
        {
            _context = context;
            _emailService = emailService;
            _notificationService = notificationService;
        }

        // GET: /Admin
        public async Task<IActionResult> Index()
        {
            var cars = await _context.Cars.OrderBy(c => c.Brand).ThenBy(c => c.Model).ThenBy(c => c.Location).ToListAsync();

            // Revenue calculations
            var allBookings = await _context.RentalBookings
                .IgnoreQueryFilters()
                .Include(b => b.User)
                .ToListAsync();

            decimal grossRevenue = allBookings
                .Where(b => b.Status == "Active" || b.Status == "Canceled" || b.Status == "CanceledByAdmin")
                .Sum(b => b.TotalPrice);

            // Expenses = refunds + welcome bonuses
            decimal totalRefunds = allBookings
                .Where(b => b.Status == "Canceled" || b.Status == "CanceledByAdmin")
                .Sum(b => b.RefundAmount ?? 0);

            // Welcome bonuses: €20 per client user
            int clientCount = await _context.Users.CountAsync(u => u.Role == "Client");
            decimal welcomeBonuses = clientCount * 20.00m;

            decimal totalExpenses = totalRefunds + welcomeBonuses;

            // Net revenue cannot go below 0 in display
            decimal netRevenue = grossRevenue - totalExpenses;

            ViewBag.GrossRevenue = grossRevenue;
            ViewBag.TotalExpenses = totalExpenses;
            ViewBag.TotalRefunds = totalRefunds;
            ViewBag.WelcomeBonuses = welcomeBonuses;
            ViewBag.NetRevenue = netRevenue;
            ViewBag.TotalBookings = allBookings.Count;
            ViewBag.ActiveBookings = allBookings.Count(b => b.Status == "Active");
            ViewBag.CanceledBookings = allBookings.Count(b => b.Status != "Active");

            return View(cars);
        }

        // GET: /Admin/Create
        [HttpGet]
        public IActionResult Create()
        {
            return View(new CarFormViewModel());
        }

        // POST: /Admin/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CarFormViewModel formModel)
        {
            if (!ModelState.IsValid)
                return View(formModel);

            var car = new Car
            {
                Brand = formModel.Brand,
                Model = formModel.Model,
                PricePerDay = formModel.PricePerDay,
                Description = formModel.Description,
                ImageUrl = formModel.ImageUrl,
                Seats = formModel.Seats,
                Location = formModel.Location,
                LicensePlate = formModel.LicensePlate,
                Chassis = formModel.Chassis,
                FuelConsumption = formModel.FuelConsumption,
                FabricationYear = formModel.FabricationYear,
                EngineType = formModel.EngineType,
                Transmission = formModel.Transmission,
                Horsepower = formModel.Horsepower
            };

            _context.Cars.Add(car);
            await _context.SaveChangesAsync();

            TempData["Success"] = $"Car '{car.Brand} {car.Model}' ({car.LicensePlate}) created successfully.";
            return RedirectToAction(nameof(Index));
        }

        // GET: /Admin/Edit/5
        [HttpGet]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();
            var car = await _context.Cars.FindAsync(id);
            if (car == null) return NotFound();

            var model = new CarFormViewModel
            {
                Id = car.Id,
                Brand = car.Brand,
                Model = car.Model,
                PricePerDay = car.PricePerDay,
                Description = car.Description,
                ImageUrl = car.ImageUrl,
                Seats = car.Seats,
                Location = car.Location,
                LicensePlate = car.LicensePlate,
                Chassis = car.Chassis,
                FuelConsumption = car.FuelConsumption,
                FabricationYear = car.FabricationYear,
                EngineType = car.EngineType,
                Transmission = car.Transmission,
                Horsepower = car.Horsepower
            };

            return View(model);
        }

        // POST: /Admin/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, CarFormViewModel formModel)
        {
            if (id != formModel.Id) return NotFound();
            if (!ModelState.IsValid) return View(formModel);

            var car = await _context.Cars.FindAsync(id);
            if (car == null) return NotFound();

            car.Brand = formModel.Brand;
            car.Model = formModel.Model;
            car.PricePerDay = formModel.PricePerDay;
            car.Description = formModel.Description;
            car.ImageUrl = formModel.ImageUrl;
            car.Seats = formModel.Seats;
            car.Location = formModel.Location;
            car.LicensePlate = formModel.LicensePlate;
            car.Chassis = formModel.Chassis;
            car.FuelConsumption = formModel.FuelConsumption;
            car.FabricationYear = formModel.FabricationYear;
            car.EngineType = formModel.EngineType;
            car.Transmission = formModel.Transmission;
            car.Horsepower = formModel.Horsepower;

            _context.Cars.Update(car);
            await _context.SaveChangesAsync();

            TempData["Success"] = $"Car '{car.Brand} {car.Model}' ({car.LicensePlate}) updated successfully.";
            return RedirectToAction(nameof(Index));
        }

        // GET: /Admin/Delete/5
        [HttpGet]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();
            var car = await _context.Cars
                .Include(c => c.UnavailabilityPeriods)
                .FirstOrDefaultAsync(c => c.Id == id);
            if (car == null) return NotFound();

            // Check if car is currently marked unavailable
            bool isCurrentlyUnavailable = car.UnavailabilityPeriods
                .Any(u => u.StartDate <= DateTime.Today && u.EndDate >= DateTime.Today);

            ViewBag.IsCurrentlyUnavailable = isCurrentlyUnavailable;
            return View(car);
        }

        // POST: /Admin/DeleteConfirmed/5
        /// <summary>
        /// Soft-deletes a car. Requires the car to be currently marked unavailable.
        /// All future active bookings are auto-canceled with 120% refund to client balance.
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var car = await _context.Cars
                .Include(c => c.UnavailabilityPeriods)
                .FirstOrDefaultAsync(c => c.Id == id);
            if (car == null) return NotFound();

            // Verify the car is currently marked unavailable
            bool isCurrentlyUnavailable = car.UnavailabilityPeriods
                .Any(u => u.StartDate <= DateTime.Today && u.EndDate >= DateTime.Today);

            if (!isCurrentlyUnavailable)
            {
                TempData["Error"] = "You must first mark this car as unavailable before deleting it. Go to the car's detail page and set an unavailability period.";
                return RedirectToAction("Delete", new { id });
            }

            // Cancel all future active bookings with 120% refund
            var futureBookings = await _context.RentalBookings
                .Include(b => b.User)
                .Where(b => b.CarId == id && b.Status == "Active" && b.EndDate >= DateTime.Today)
                .ToListAsync();

            int canceledCount = 0;
            foreach (var booking in futureBookings)
            {
                booking.Status = "CanceledByAdmin";
                if (booking.User != null)
                {
                    decimal refund = Math.Round(booking.TotalPrice * 1.20m, 2);
                    booking.User.AccountBalance += refund;
                    booking.RefundAmount = refund;

                    // Notify client
                    try
                    {
                        await _emailService.SendCancellationByAdminAsync(
                            booking.User.Email, booking.User.Username,
                            $"{car.Brand} {car.Model}", booking.StartDate, booking.EndDate, refund);
                        await _notificationService.CreateNotificationAsync(
                            booking.UserId,
                            "Booking Canceled by Admin",
                            $"Your booking for {car.Brand} {car.Model} ({booking.StartDate:dd MMM} — {booking.EndDate:dd MMM yyyy}) was canceled. €{refund:F2} (120%) refunded.",
                            "Cancellation");
                    }
                    catch { }
                }
                canceledCount++;
            }

            // Soft-delete the car
            car.IsDeleted = true;
            await _context.SaveChangesAsync();

            TempData["Success"] = $"Car '{car.Brand} {car.Model}' has been deleted. {canceledCount} booking(s) were canceled with 120% refund.";
            return RedirectToAction(nameof(Index));
        }

        // POST: /Admin/SetUnavailable
        /// <summary>
        /// Admin marks a car as unavailable for a specific period.
        /// Any overlapping active client bookings are automatically canceled with 120% refund.
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SetUnavailable(int carId, DateTime startDate, DateTime endDate, string? reason)
        {
            if (endDate <= startDate)
            {
                TempData["Error"] = "End date must be after start date.";
                return RedirectToAction("Details", "Car", new { id = carId });
            }

            var car = await _context.Cars.FindAsync(carId);
            if (car == null) return NotFound();

            // Create the unavailability period
            var period = new UnavailabilityPeriod
            {
                CarId = carId,
                StartDate = startDate,
                EndDate = endDate,
                Reason = reason,
                CreatedAt = DateTime.Now
            };

            _context.UnavailabilityPeriods.Add(period);

            // Find and cancel all overlapping active client bookings with 120% refund
            var overlappingBookings = await _context.RentalBookings
                .Include(b => b.User)
                .Where(b => b.CarId == carId
                    && b.Status == "Active"
                    && b.StartDate < endDate
                    && b.EndDate > startDate)
                .ToListAsync();

            int canceledCount = 0;
            foreach (var booking in overlappingBookings)
            {
                booking.Status = "CanceledByAdmin";
                if (booking.User != null)
                {
                    decimal refund = Math.Round(booking.TotalPrice * 1.20m, 2);
                    booking.User.AccountBalance += refund;
                    booking.RefundAmount = refund;

                    try
                    {
                        await _emailService.SendCancellationByAdminAsync(
                            booking.User.Email, booking.User.Username,
                            $"{car.Brand} {car.Model}", booking.StartDate, booking.EndDate, refund);
                        await _notificationService.CreateNotificationAsync(
                            booking.UserId,
                            "Booking Canceled by Admin",
                            $"Your booking for {car.Brand} {car.Model} ({booking.StartDate:dd MMM} — {booking.EndDate:dd MMM yyyy}) was canceled due to unavailability. €{refund:F2} (120%) refunded.",
                            "Cancellation");
                    }
                    catch { }
                }
                canceledCount++;
            }

            await _context.SaveChangesAsync();

            string message = $"Car marked unavailable from {startDate:dd MMM yyyy} to {endDate:dd MMM yyyy}.";
            if (canceledCount > 0)
            {
                message += $" {canceledCount} overlapping booking(s) were automatically canceled with 120% refund.";
            }

            TempData["Success"] = message;
            return RedirectToAction("Details", "Car", new { id = carId });
        }

        // POST: /Admin/CancelBooking/5
        /// <summary>
        /// Admin cancels a specific upcoming booking. Client receives 120% refund to balance.
        /// Admin cannot cancel active (in-progress) bookings — only upcoming ones.
        /// Refund is capped so total refunds never exceed gross revenue.
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CancelBooking(int id)
        {
            var booking = await _context.RentalBookings
                .Include(b => b.User)
                .Include(b => b.Car)
                .FirstOrDefaultAsync(b => b.Id == id);

            if (booking == null) return NotFound();

            if (booking.Status != "Active")
            {
                TempData["Error"] = "This booking is already canceled.";
                return RedirectToAction("RentalLog");
            }

            // Admin can only cancel upcoming bookings, not active ones
            if (booking.StartDate <= DateTime.Today)
            {
                TempData["Error"] = "Cannot cancel an active or past booking. Only upcoming bookings can be canceled by admin.";
                return RedirectToAction("RentalLog");
            }

            decimal refund = Math.Round(booking.TotalPrice * 1.20m, 2);

            // Cap refund: total refunds should not exceed gross revenue
            var allBookings = await _context.RentalBookings.IgnoreQueryFilters().ToListAsync();
            decimal grossRevenue = allBookings.Sum(b => b.TotalPrice);
            decimal existingRefunds = allBookings.Where(b => b.RefundAmount.HasValue).Sum(b => b.RefundAmount!.Value);

            if (existingRefunds + refund > grossRevenue)
            {
                refund = Math.Max(0, grossRevenue - existingRefunds);
                if (refund <= 0)
                {
                    TempData["Error"] = "Cannot process refund — total refunds would exceed gross revenue.";
                    return RedirectToAction("RentalLog");
                }
            }

            booking.Status = "CanceledByAdmin";
            booking.RefundAmount = refund;

            if (booking.User != null)
            {
                booking.User.AccountBalance += refund;
                TempData["Success"] = $"Booking #{booking.Id} canceled. €{refund:F2} (120%) refunded to {booking.User.Username}'s balance.";

                // Send email + notification to client
                try
                {
                    var carName = $"{booking.Car?.Brand} {booking.Car?.Model}";
                    await _emailService.SendCancellationByAdminAsync(
                        booking.User.Email, booking.User.Username,
                        carName, booking.StartDate, booking.EndDate, refund);
                    await _notificationService.CreateNotificationAsync(
                        booking.UserId,
                        "Booking Canceled by Admin",
                        $"Your booking #{booking.Id} for {carName} ({booking.StartDate:dd MMM} — {booking.EndDate:dd MMM yyyy}) was canceled by admin. €{refund:F2} (120%) credited to your balance.",
                        "Cancellation");
                }
                catch { }
            }
            else
            {
                TempData["Success"] = $"Booking #{booking.Id} canceled.";
            }

            await _context.SaveChangesAsync();
            return RedirectToAction("RentalLog");
        }

        // GET: /Admin/RentalLog?period=month
        /// <summary>
        /// Displays a filterable table of ALL rental bookings across the site.
        /// Uses IgnoreQueryFilters to include bookings for soft-deleted cars.
        /// Also includes unavailability periods for a complete audit trail.
        /// </summary>
        public async Task<IActionResult> RentalLog(string? period)
        {
            IQueryable<RentalBooking> query = _context.RentalBookings
                .IgnoreQueryFilters()  // Show bookings for deleted cars too
                .Include(b => b.Car)
                .Include(b => b.User);

            DateTime? cutoffDate = period switch
            {
                "week" => DateTime.Today.AddDays(-7),
                "month" => DateTime.Today.AddMonths(-1),
                "year" => DateTime.Today.AddYears(-1),
                _ => null
            };

            if (cutoffDate.HasValue)
                query = query.Where(b => b.StartDate >= cutoffDate.Value);

            var rentals = await query.OrderByDescending(b => b.CreatedAt).ToListAsync();

            // Also fetch unavailability periods for the log
            IQueryable<UnavailabilityPeriod> unavailQuery = _context.UnavailabilityPeriods
                .IgnoreQueryFilters()
                .Include(u => u.Car);

            if (cutoffDate.HasValue)
                unavailQuery = unavailQuery.Where(u => u.StartDate >= cutoffDate.Value);

            var unavailPeriods = await unavailQuery.OrderByDescending(u => u.CreatedAt).ToListAsync();

            ViewBag.CurrentPeriod = period ?? "all";
            ViewBag.UnavailabilityPeriods = unavailPeriods;
            return View(rentals);
        }
    }
}
