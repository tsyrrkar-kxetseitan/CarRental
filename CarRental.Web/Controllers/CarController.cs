using CarRental.Core.Entities;
using CarRental.Data;
using CarRental.Web.Services;
using CarRental.Web.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace CarRental.Web.Controllers
{
    /// <summary>
    /// Handles car listing, details, calendar-based rental booking,
    /// cart/payment, client rental history, and client booking cancellation.
    /// </summary>
    public class CarController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IEmailService _emailService;
        private readonly INotificationService _notificationService;
        private const string CartSessionKey = "ShoppingCart";

        public CarController(ApplicationDbContext context, IEmailService emailService, INotificationService notificationService)
        {
            _context = context;
            _emailService = emailService;
            _notificationService = notificationService;
        }

        // ─────────────────────────── Browse Cars ───────────────────────────
        // GET: /Car/Index?query=BMW+X5&location=Cluj-Napoca&seats=5
        [HttpGet]
        public async Task<IActionResult> Index(SearchViewModel search)
        {
            IQueryable<Car> query = _context.Cars;

            // Location filter (mandatory if provided, case-insensitive)
            if (!string.IsNullOrWhiteSpace(search.Location))
                query = query.Where(c => c.Location.ToLower() == search.Location.ToLower());

            // Seats filter
            if (search.Seats.HasValue)
                query = query.Where(c => c.Seats == search.Seats.Value);

            // Brand filter (case-insensitive)
            if (!string.IsNullOrWhiteSpace(search.Brand))
                query = query.Where(c => c.Brand.ToLower() == search.Brand.ToLower());

            // Free-text search: AND logic on each word (case-insensitive)
            if (!string.IsNullOrWhiteSpace(search.Query))
            {
                var terms = search.Query.Trim().Split(' ', StringSplitOptions.RemoveEmptyEntries);
                foreach (var term in terms)
                {
                    var t = term.ToLower(); // closure capture, case-insensitive
                    query = query.Where(c =>
                        c.Brand.ToLower().Contains(t) ||
                        c.Model.ToLower().Contains(t) ||
                        (c.Description != null && c.Description.ToLower().Contains(t)));
                }
            }

            // Date-based availability filter
            if (search.StartDate.HasValue && search.EndDate.HasValue)
            {
                var start = search.StartDate.Value;
                var end = search.EndDate.Value;

                // Exclude cars that have overlapping active bookings
                var bookedCarIds = await _context.RentalBookings
                    .Where(b => b.Status == "Active" && b.StartDate < end && b.EndDate > start)
                    .Select(b => b.CarId)
                    .Distinct()
                    .ToListAsync();

                // Exclude cars that have overlapping unavailability
                var unavailCarIds = await _context.UnavailabilityPeriods
                    .Where(u => u.StartDate < end && u.EndDate > start)
                    .Select(u => u.CarId)
                    .Distinct()
                    .ToListAsync();

                var excludedIds = bookedCarIds.Union(unavailCarIds).ToHashSet();

                // Keep cars that have at least one unit available in the target location
                // (don't exclude the entire model — just specific car IDs)
                query = query.Where(c => !excludedIds.Contains(c.Id));
            }

            // Group by Brand+Model to show distinct models (with count of available units)
            var cars = await query.OrderBy(c => c.Brand).ThenBy(c => c.Model).ThenBy(c => c.Location).ToListAsync();

            ViewBag.Search = search;
            ViewBag.Locations = await _context.Cars.Select(c => c.Location).Distinct().OrderBy(l => l).ToListAsync();
            ViewBag.Brands = await _context.Cars.Select(c => c.Brand).Distinct().OrderBy(b => b).ToListAsync();
            ViewBag.CartCount = GetCart().Count;

            return View(cars);
        }

        // ─────────────────────────── Car Details ───────────────────────────
        // GET: /Car/Details?brand=BMW&model=X5  (or /Car/Details/5 for backward compat)
        [HttpGet]
        public async Task<IActionResult> Details(int? id, string? brand, string? model,
            string? location, string? plate, DateTime? searchStart, DateTime? searchEnd)
        {
            Car? car = null;

            if (id.HasValue)
            {
                car = await _context.Cars.FirstOrDefaultAsync(c => c.Id == id.Value);
                if (car != null)
                {
                    brand = car.Brand;
                    model = car.Model;
                }
            }

            if (string.IsNullOrEmpty(brand) || string.IsNullOrEmpty(model))
                return NotFound();

            // Get all non-deleted cars of this Brand+Model
            var allCars = await _context.Cars
                .Where(c => c.Brand == brand && c.Model == model)
                .OrderBy(c => c.Location).ThenBy(c => c.LicensePlate)
                .ToListAsync();

            if (!allCars.Any()) return NotFound();

            // Get distinct locations for this model
            var locations = allCars.Select(c => c.Location).Distinct().OrderBy(l => l).ToList();

            // If plate is specified, select that car; otherwise pick first available
            if (!string.IsNullOrEmpty(plate))
                car = allCars.FirstOrDefault(c => c.LicensePlate == plate);
            else if (!string.IsNullOrEmpty(location))
                car = allCars.FirstOrDefault(c => c.Location == location);

            // If search dates provided, try to find an available car automatically
            if (car == null && searchStart.HasValue && searchEnd.HasValue && !string.IsNullOrEmpty(location))
            {
                var carsInLocation = allCars.Where(c => c.Location == location).ToList();
                foreach (var candidate in carsInLocation)
                {
                    bool booked = await _context.RentalBookings.AnyAsync(b =>
                        b.CarId == candidate.Id && b.Status == "Active" &&
                        b.StartDate < searchEnd.Value && b.EndDate > searchStart.Value);
                    bool unavail = await _context.UnavailabilityPeriods.AnyAsync(u =>
                        u.CarId == candidate.Id &&
                        u.StartDate < searchEnd.Value && u.EndDate > searchStart.Value);
                    if (!booked && !unavail)
                    {
                        car = candidate;
                        break;
                    }
                }
            }

            car ??= allCars.First();

            // Cars in the selected location
            var carsForLocation = allCars
                .Where(c => c.Location == car.Location)
                .ToList();

            ViewBag.IsAdmin = User.IsInRole("Admin");
            ViewBag.IsAuthenticated = User.Identity?.IsAuthenticated == true;
            ViewBag.AllCars = allCars;
            ViewBag.Locations = locations;
            ViewBag.CarsForLocation = carsForLocation;
            ViewBag.SelectedLocation = car.Location;
            ViewBag.SelectedPlate = car.LicensePlate;
            ViewBag.SearchStart = searchStart?.ToString("yyyy-MM-dd");
            ViewBag.SearchEnd = searchEnd?.ToString("yyyy-MM-dd");
            ViewBag.CartCount = GetCart().Count;

            return View(car);
        }

        // GET: /Car/GetCarsForLocation?brand=BMW&model=X5&location=Cluj-Napoca
        [HttpGet]
        public async Task<IActionResult> GetCarsForLocation(string brand, string model, string location)
        {
            var cars = await _context.Cars
                .Where(c => c.Brand == brand && c.Model == model && c.Location == location)
                .Select(c => new { c.Id, c.LicensePlate })
                .ToListAsync();

            return Json(cars);
        }

        // GET: /Car/GetBookings/5?month=7&year=2026
        /// <summary>
        /// API endpoint returning JSON booking + unavailability data for the calendar.
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetBookings(int id, int month, int year)
        {
            var startOfMonth = new DateTime(year, month, 1);
            var endOfMonth = startOfMonth.AddMonths(1).AddDays(-1);

            // Fetch active bookings that overlap with the month
            var bookings = await _context.RentalBookings
                .Include(b => b.User)
                .Where(b => b.CarId == id
                    && b.Status == "Active"
                    && b.StartDate <= endOfMonth
                    && b.EndDate >= startOfMonth)
                .OrderBy(b => b.StartDate)
                .Select(b => new
                {
                    type = "booking",
                    startDate = b.StartDate.ToString("yyyy-MM-dd"),
                    endDate = b.EndDate.ToString("yyyy-MM-dd"),
                    username = User.IsInRole("Admin") ? (b.User != null ? b.User.Username : "Unknown") : (string?)null
                })
                .ToListAsync();

            // Fetch unavailability periods that overlap with the month
            var unavailable = await _context.UnavailabilityPeriods
                .Where(u => u.CarId == id
                    && u.StartDate <= endOfMonth
                    && u.EndDate >= startOfMonth)
                .OrderBy(u => u.StartDate)
                .Select(u => new
                {
                    type = "unavailable",
                    startDate = u.StartDate.ToString("yyyy-MM-dd"),
                    endDate = u.EndDate.ToString("yyyy-MM-dd"),
                    username = (string?)(u.Reason ?? "Unavailable")
                })
                .ToListAsync();

            var result = bookings.Cast<object>().Concat(unavailable.Cast<object>());
            return Json(result);
        }

        // ─────────────────────────── Add to Cart ───────────────────────────
        // POST: /Car/AddToCart
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Client")]
        public async Task<IActionResult> AddToCart(RentCarViewModel model)
        {
            if (model.EndDate <= model.StartDate)
            {
                TempData["Error"] = "End date must be after start date.";
                return RedirectToAction("Details", new { id = model.CarId });
            }
            if (model.StartDate < DateTime.Today)
            {
                TempData["Error"] = "Start date cannot be in the past.";
                return RedirectToAction("Details", new { id = model.CarId });
            }

            var car = await _context.Cars.FindAsync(model.CarId);
            if (car == null) return NotFound();

            // Check overlap with active bookings
            bool hasBookingOverlap = await _context.RentalBookings
                .AnyAsync(b => b.CarId == model.CarId
                    && b.Status == "Active"
                    && b.StartDate < model.EndDate
                    && b.EndDate > model.StartDate);

            if (hasBookingOverlap)
            {
                TempData["Error"] = "The selected dates overlap with an existing booking.";
                return RedirectToAction("Details", new { id = model.CarId });
            }

            // Check overlap with unavailability periods
            bool hasUnavailableOverlap = await _context.UnavailabilityPeriods
                .AnyAsync(u => u.CarId == model.CarId
                    && u.StartDate < model.EndDate
                    && u.EndDate > model.StartDate);

            if (hasUnavailableOverlap)
            {
                TempData["Error"] = "The car is unavailable during the selected dates.";
                return RedirectToAction("Details", new { id = model.CarId });
            }

            // Check if same car+dates is already in cart
            var cart = GetCart();
            bool alreadyInCart = cart.Any(ci =>
                ci.CarId == model.CarId &&
                ci.StartDate == model.StartDate &&
                ci.EndDate == model.EndDate);

            if (alreadyInCart)
            {
                TempData["Error"] = "This booking is already in your cart.";
                return RedirectToAction("Details", new { id = model.CarId });
            }

            int totalDays = (model.EndDate - model.StartDate).Days;
            decimal totalPrice = CalculateTotalPrice(totalDays, car.PricePerDay);

            var item = new CartItem
            {
                CarId = car.Id,
                CarBrand = car.Brand,
                CarModel = car.Model,
                LicensePlate = car.LicensePlate,
                Location = car.Location,
                PricePerDay = car.PricePerDay,
                StartDate = model.StartDate,
                EndDate = model.EndDate,
                TotalDays = totalDays,
                TotalPrice = totalPrice
            };

            cart.Add(item);
            SaveCart(cart);

            TempData["Success"] = $"{car.Brand} {car.Model} ({car.LicensePlate}) added to cart for {totalDays} day(s) — €{totalPrice:F2}";
            return RedirectToAction("Cart");
        }

        // POST: /Car/RemoveFromCart?index=0
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Client")]
        public IActionResult RemoveFromCart(int index)
        {
            var cart = GetCart();
            if (index >= 0 && index < cart.Count)
            {
                cart.RemoveAt(index);
                SaveCart(cart);
            }
            return RedirectToAction("Cart");
        }

        // ─────────────────────────── Cart Page ───────────────────────────
        // GET: /Car/Cart
        [Authorize(Roles = "Client")]
        public async Task<IActionResult> Cart()
        {
            var userId = GetUserId();
            if (userId == null) return RedirectToAction("Login", "Account");

            var user = await _context.Users.FindAsync(userId.Value);
            if (user == null) return RedirectToAction("Login", "Account");

            var vm = new CartViewModel
            {
                Items = GetCart(),
                AccountBalance = user.AccountBalance
            };

            return View(vm);
        }

        // POST: /Car/ConfirmPayment
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Client")]
        public async Task<IActionResult> ConfirmPayment(PaymentFormModel payment)
        {
            var userId = GetUserId();
            if (userId == null) return RedirectToAction("Login", "Account");

            var user = await _context.Users.FindAsync(userId.Value);
            if (user == null) return RedirectToAction("Login", "Account");

            var cart = GetCart();
            if (!cart.Any())
            {
                TempData["Error"] = "Your cart is empty.";
                return RedirectToAction("Cart");
            }

            // Server-side payment validation
            if (string.IsNullOrWhiteSpace(payment.CardNumber) ||
                string.IsNullOrWhiteSpace(payment.CardHolder) ||
                string.IsNullOrWhiteSpace(payment.ExpiryDate) ||
                string.IsNullOrWhiteSpace(payment.Cvv))
            {
                TempData["Error"] = "Please fill in all payment details.";
                return RedirectToAction("Cart");
            }

            // Strip spaces from card number for validation
            var cardDigits = payment.CardNumber.Replace(" ", "");
            if (!Regex.IsMatch(cardDigits, @"^\d{16}$"))
            {
                TempData["Error"] = "Card number must be exactly 16 digits.";
                return RedirectToAction("Cart");
            }

            if (!Regex.IsMatch(payment.CardHolder, @"^[a-zA-ZÀ-ÿ\s\-'\.]+$"))
            {
                TempData["Error"] = "Cardholder name must contain only letters.";
                return RedirectToAction("Cart");
            }

            if (!Regex.IsMatch(payment.ExpiryDate, @"^(0[1-9]|1[0-2])\/\d{2}$"))
            {
                TempData["Error"] = "Expiry date must be in MM/YY format with a valid month.";
                return RedirectToAction("Cart");
            }
            else
            {
                // Check expiry is not in the past
                var parts = payment.ExpiryDate.Split('/');
                int month = int.Parse(parts[0]);
                int year = 2000 + int.Parse(parts[1]);
                var expiryDate = new DateTime(year, month, 1).AddMonths(1).AddDays(-1);
                if (expiryDate < DateTime.Today)
                {
                    TempData["Error"] = "Card has expired. Please use a valid card.";
                    return RedirectToAction("Cart");
                }
            }

            if (!Regex.IsMatch(payment.Cvv, @"^\d{3}$"))
            {
                TempData["Error"] = "CVV must be exactly 3 digits.";
                return RedirectToAction("Cart");
            }

            decimal subtotal = cart.Sum(i => i.TotalPrice);
            decimal balanceDeduction = 0;

            if (payment.UseBalance)
            {
                balanceDeduction = Math.Min(user.AccountBalance, subtotal);
                user.AccountBalance -= balanceDeduction;
            }

            // Re-check availability for each cart item before confirming
            foreach (var item in cart)
            {
                bool overlap = await _context.RentalBookings
                    .AnyAsync(b => b.CarId == item.CarId
                        && b.Status == "Active"
                        && b.StartDate < item.EndDate
                        && b.EndDate > item.StartDate);

                bool unavail = await _context.UnavailabilityPeriods
                    .AnyAsync(u => u.CarId == item.CarId
                        && u.StartDate < item.EndDate
                        && u.EndDate > item.StartDate);

                if (overlap || unavail)
                {
                    // Revert balance
                    user.AccountBalance += balanceDeduction;
                    TempData["Error"] = $"{item.CarBrand} {item.CarModel} ({item.LicensePlate}) is no longer available for the selected dates. Please update your cart.";
                    return RedirectToAction("Cart");
                }
            }

            // Generate order group ID
            string orderGroup = $"ORD-{DateTime.Now:yyyyMMddHHmmss}-{userId}";

            // Create all bookings
            foreach (var item in cart)
            {
                var booking = new RentalBooking
                {
                    UserId = userId.Value,
                    CarId = item.CarId,
                    StartDate = item.StartDate,
                    EndDate = item.EndDate,
                    TotalPrice = item.TotalPrice,
                    Status = "Active",
                    CreatedAt = DateTime.Now
                };
                _context.RentalBookings.Add(booking);
            }

            await _context.SaveChangesAsync();

            // Clear cart
            SaveCart(new List<CartItem>());

            decimal amountCharged = subtotal - balanceDeduction;
            TempData["PaymentSuccess"] = $"Payment of €{amountCharged:F2} processed successfully! " +
                (balanceDeduction > 0 ? $"€{balanceDeduction:F2} was deducted from your account balance. " : "") +
                $"{cart.Count} booking(s) confirmed.";

            // Send booking confirmation email and create notifications
            try
            {
                var bookingInfos = cart.Select(c => new BookingEmailInfo
                {
                    CarName = $"{c.CarBrand} {c.CarModel}",
                    LicensePlate = c.LicensePlate,
                    Location = c.Location,
                    StartDate = c.StartDate,
                    EndDate = c.EndDate,
                    TotalPrice = c.TotalPrice
                }).ToList();

                await _emailService.SendBookingConfirmationAsync(user.Email, user.Username, bookingInfos, amountCharged);

                var carNames = string.Join(", ", cart.Select(c => $"{c.CarBrand} {c.CarModel}"));
                await _notificationService.CreateNotificationAsync(
                    userId.Value,
                    "Booking Confirmed",
                    $"Your booking for {carNames} has been confirmed. Total charged: €{amountCharged:F2}.",
                    "Booking");
            }
            catch { /* Email failure should not block the booking */ }

            return RedirectToAction("Cart");
        }

        // ─────────────────────────── Wikipedia Car Image ───────────────────
        // GET: /Car/GetWikiImage?brand=BMW&model=X5
        [HttpGet]
        public async Task<IActionResult> GetWikiImage(string brand, string model)
        {
            try
            {
                using var httpClient = new HttpClient();
                httpClient.DefaultRequestHeaders.Add("User-Agent", "RoadRunners-CarRental/1.0");

                var searchQuery = $"{brand} {model} car";
                var apiUrl = $"https://en.wikipedia.org/api/rest_v1/page/summary/{Uri.EscapeDataString($"{brand} {model}")}";

                var response = await httpClient.GetAsync(apiUrl);
                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    using var doc = JsonDocument.Parse(json);
                    if (doc.RootElement.TryGetProperty("thumbnail", out var thumbnail) &&
                        thumbnail.TryGetProperty("source", out var source))
                    {
                        return Json(new { imageUrl = source.GetString() });
                    }
                }

                // Fallback: try with different search terms
                var fallbackTerms = new List<string> { $"{brand}_{model}", $"{brand}_{model}_(car)" };

                // Special handling for models like "Series 3" → "3_Series" (Wikipedia naming)
                if (model.StartsWith("Series ", StringComparison.OrdinalIgnoreCase))
                {
                    var seriesNumber = model.Substring("Series ".Length).Trim();
                    fallbackTerms.Insert(0, $"{brand}_{seriesNumber}_Series");
                }

                foreach (var term in fallbackTerms)
                {
                    var fallbackUrl = $"https://en.wikipedia.org/api/rest_v1/page/summary/{Uri.EscapeDataString(term)}";
                    var fallbackResponse = await httpClient.GetAsync(fallbackUrl);
                    if (fallbackResponse.IsSuccessStatusCode)
                    {
                        var json = await fallbackResponse.Content.ReadAsStringAsync();
                        using var doc = JsonDocument.Parse(json);
                        if (doc.RootElement.TryGetProperty("thumbnail", out var thumbnail) &&
                            thumbnail.TryGetProperty("source", out var source))
                        {
                            return Json(new { imageUrl = source.GetString() });
                        }
                    }
                }

                return Json(new { imageUrl = (string?)null });
            }
            catch
            {
                return Json(new { imageUrl = (string?)null });
            }
        }

        // ─────────────────────────── Cancel Booking ───────────────────────
        // POST: /Car/CancelBooking/5
        /// <summary>
        /// Client cancels their own booking with refund logic:
        /// - Upcoming, 3+ days before start: 100% refund to balance
        /// - Upcoming, less than 3 days before start: 85% refund to balance
        /// - Active (in progress): pro-rated refund for remaining unused days
        ///   (calculated using the tiered discount formula, capped at total paid)
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Client")]
        public async Task<IActionResult> CancelBooking(int id)
        {
            var userId = GetUserId();
            if (userId == null) return RedirectToAction("Login", "Account");

            var booking = await _context.RentalBookings
                .Include(b => b.Car)
                .FirstOrDefaultAsync(b => b.Id == id && b.UserId == userId.Value);

            if (booking == null) return NotFound();

            if (booking.Status != "Active")
            {
                TempData["Error"] = "This booking is already canceled.";
                return RedirectToAction("MyAccount", "Account");
            }

            // Past bookings cannot be canceled
            if (booking.EndDate < DateTime.Today)
            {
                TempData["Error"] = "Cannot cancel a completed booking.";
                return RedirectToAction("MyAccount", "Account");
            }

            var user = await _context.Users.FindAsync(userId.Value);
            if (user == null) return NotFound();

            decimal refundAmount;
            string refundMessage;
            bool isCurrentlyActive = booking.StartDate <= DateTime.Today && booking.EndDate >= DateTime.Today;

            if (isCurrentlyActive)
            {
                // Pro-rated refund: refund the value of remaining unused days
                int totalDays = (booking.EndDate - booking.StartDate).Days;
                int usedDays = (DateTime.Today - booking.StartDate).Days; // days already elapsed
                int remainingDays = totalDays - usedDays;

                if (remainingDays <= 0)
                {
                    TempData["Error"] = "This booking ends today and cannot be canceled.";
                    return RedirectToAction("MyAccount", "Account");
                }

                // Calculate what the used portion cost using the tiered pricing
                decimal pricePerDay = booking.Car?.PricePerDay ?? (totalDays > 0 ? booking.TotalPrice / totalDays : 0);
                decimal usedCost = CalculateTotalPrice(usedDays, pricePerDay);

                // Refund = total paid minus what was used, but never negative
                refundAmount = Math.Max(0, Math.Round(booking.TotalPrice - usedCost, 2));

                // Safety cap: never refund more than what was paid
                refundAmount = Math.Min(refundAmount, booking.TotalPrice);

                refundMessage = $"Pro-rated refund for {remainingDays} remaining day(s)";
            }
            else
            {
                // Upcoming booking — standard refund tiers
                int daysUntilStart = (booking.StartDate - DateTime.Today).Days;
                decimal refundPercent;

                if (daysUntilStart >= 3)
                {
                    refundPercent = 1.00m; // 100%
                    refundMessage = "Full refund (100%)";
                }
                else
                {
                    refundPercent = 0.85m; // 85%
                    refundMessage = "Partial refund (85% — less than 3 days notice)";
                }

                refundAmount = Math.Round(booking.TotalPrice * refundPercent, 2);
            }

            user.AccountBalance += refundAmount;
            booking.Status = "Canceled";
            booking.RefundAmount = refundAmount;

            await _context.SaveChangesAsync();

            // Create notification for the client
            var carName = $"{booking.Car?.Brand} {booking.Car?.Model}";
            await _notificationService.CreateNotificationAsync(
                userId.Value,
                "Booking Canceled",
                $"Your booking #{booking.Id} for {carName} has been canceled. {refundMessage}: €{refundAmount:F2} credited to your balance.",
                "Cancellation");

            TempData["Success"] = $"Booking #{booking.Id} canceled. {refundMessage}: €{refundAmount:F2} credited to your account balance.";
            return RedirectToAction("MyAccount", "Account");
        }

        // ─────────────────────────── Booking Confirmation ─────────────────
        // GET: /Car/BookingConfirmation/5
        [Authorize]
        public async Task<IActionResult> BookingConfirmation(int id)
        {
            var booking = await _context.RentalBookings
                .Include(b => b.Car)
                .Include(b => b.User)
                .FirstOrDefaultAsync(b => b.Id == id);

            if (booking == null) return NotFound();
            return View(booking);
        }

        // ─────────────────────────── Helpers ──────────────────────────────

        /// <summary>
        /// Calculates the total rental price with tiered discounts.
        /// First 3 days: full price.
        /// Day 4: 5% discount, Day 5: 10%, ... up to max 40% discount.
        /// </summary>
        public static decimal CalculateTotalPrice(int totalDays, decimal pricePerDay)
        {
            decimal total = 0;
            for (int day = 1; day <= totalDays; day++)
            {
                if (day <= 3)
                {
                    total += pricePerDay;
                }
                else
                {
                    // 5% discount per day beyond 3, max 40%
                    int discountSteps = day - 3;
                    decimal discountPercent = Math.Min(discountSteps * 5m, 40m);
                    decimal discountedPrice = pricePerDay * (1 - discountPercent / 100m);
                    total += discountedPrice;
                }
            }
            return Math.Round(total, 2);
        }

        private int? GetUserId()
        {
            var claim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return int.TryParse(claim, out int id) ? id : null;
        }

        private List<CartItem> GetCart()
        {
            var json = HttpContext.Session.GetString(CartSessionKey);
            return string.IsNullOrEmpty(json)
                ? new List<CartItem>()
                : JsonSerializer.Deserialize<List<CartItem>>(json) ?? new List<CartItem>();
        }

        private void SaveCart(List<CartItem> cart)
        {
            HttpContext.Session.SetString(CartSessionKey, JsonSerializer.Serialize(cart));
        }
    }
}
