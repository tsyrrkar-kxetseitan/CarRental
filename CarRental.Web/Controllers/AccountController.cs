using CarRental.Core.Entities;
using CarRental.Data;
using CarRental.Web.Services;
using CarRental.Web.ViewModels;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace CarRental.Web.Controllers
{
    /// <summary>
    /// Handles user authentication (login/logout/register) and account management.
    /// </summary>
    public class AccountController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IEmailService _emailService;
        private readonly INotificationService _notificationService;

        public AccountController(ApplicationDbContext context, IEmailService emailService, INotificationService notificationService)
        {
            _context = context;
            _emailService = emailService;
            _notificationService = notificationService;
        }

        // GET: /Account/Login
        [HttpGet]
        public IActionResult Login()
        {
            if (User.Identity?.IsAuthenticated == true)
                return RedirectToAction("Index", "Car");
            return View();
        }

        // POST: /Account/Login
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var passwordHash = ComputeSha256Hash(model.Password);
            var user = _context.Users
                .FirstOrDefault(u => u.Username == model.Username && u.PasswordHash == passwordHash);

            if (user == null)
            {
                ModelState.AddModelError(string.Empty, "Invalid username or password.");
                return View(model);
            }

            await SignInUser(user);

            return user.Role == "Admin"
                ? RedirectToAction("Index", "Admin")
                : RedirectToAction("Index", "Car");
        }

        // GET: /Account/Register
        [HttpGet]
        public IActionResult Register()
        {
            if (User.Identity?.IsAuthenticated == true)
                return RedirectToAction("Index", "Car");
            return View();
        }

        // POST: /Account/Register
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            // Check if username already exists
            var existingUser = await _context.Users
                .FirstOrDefaultAsync(u => u.Username == model.Username);

            if (existingUser != null)
            {
                ModelState.AddModelError("Username", "This username is already taken.");
                return View(model);
            }

            // Check if email already exists
            var existingEmail = await _context.Users
                .FirstOrDefaultAsync(u => u.Email == model.Email);

            if (existingEmail != null)
            {
                ModelState.AddModelError("Email", "This email address is already registered.");
                return View(model);
            }

            // Create new Client user with €20 welcome bonus
            var user = new User
            {
                Username = model.Username,
                Email = model.Email,
                PasswordHash = ComputeSha256Hash(model.Password),
                Role = "Client",
                AccountBalance = 20.00m
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            // Send welcome email
            await _emailService.SendWelcomeEmailAsync(user.Email, user.Username);

            // Create welcome notification
            await _notificationService.CreateNotificationAsync(
                user.Id,
                "Welcome to RoadRunners!",
                $"Your account has been created with a €20.00 welcome bonus. Start browsing our fleet!",
                "Welcome");

            // Auto sign-in after registration
            await SignInUser(user);

            TempData["Success"] = $"Welcome, {user.Username}! Your account has been created with a €20.00 welcome bonus.";
            return RedirectToAction("MyAccount");
        }

        // GET: /Account/MyAccount
        /// <summary>
        /// Account page with user data, balance, and full rental history.
        /// </summary>
        [Authorize(Roles = "Client")]
        public async Task<IActionResult> MyAccount()
        {
            var userId = GetUserId();
            if (userId == null) return RedirectToAction("Login");

            var user = await _context.Users.FindAsync(userId.Value);
            if (user == null) return RedirectToAction("Login");

            // Include soft-deleted cars so history is preserved
            var rentals = await _context.RentalBookings
                .IgnoreQueryFilters()
                .Include(b => b.Car)
                .Where(b => b.UserId == userId.Value)
                .OrderByDescending(b => b.StartDate)
                .ToListAsync();

            var vm = new AccountViewModel
            {
                UserId = user.Id,
                Username = user.Username,
                Role = user.Role,
                AccountBalance = user.AccountBalance,
                RentalHistory = rentals
            };

            return View(vm);
        }

        // POST: /Account/Logout
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Login");
        }

        /// <summary>
        /// Signs in a user by creating an authentication cookie with role claims.
        /// </summary>
        private async Task SignInUser(User user)
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, user.Username),
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Role, user.Role)
            };

            var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var authProps = new AuthenticationProperties
            {
                IsPersistent = true,
                ExpiresUtc = DateTimeOffset.UtcNow.AddHours(2)
            };

            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                new ClaimsPrincipal(identity),
                authProps);
        }

        private int? GetUserId()
        {
            var claim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return int.TryParse(claim, out int id) ? id : null;
        }

        private static string ComputeSha256Hash(string rawData)
        {
            var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(rawData));
            var sb = new StringBuilder();
            foreach (var b in bytes) sb.Append(b.ToString("x2"));
            return sb.ToString();
        }
    }
}
