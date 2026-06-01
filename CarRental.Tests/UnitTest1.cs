using CarRental.Core.Entities;
using CarRental.Data;
using CarRental.Web.Controllers;
using Microsoft.EntityFrameworkCore;

namespace CarRental.Tests
{
    /// <summary>
    /// Unit tests for the CarRental application.
    /// Uses in-memory SQLite for database-dependent tests.
    /// </summary>
    public class PricingTests
    {
        [Fact]
        public void CalculateTotalPrice_SingleDay_FullPrice()
        {
            var result = CarController.CalculateTotalPrice(1, 100m);
            Assert.Equal(100m, result);
        }

        [Fact]
        public void CalculateTotalPrice_ThreeDays_NoDiscount()
        {
            // 3 days × €100 = €300
            var result = CarController.CalculateTotalPrice(3, 100m);
            Assert.Equal(300m, result);
        }

        [Fact]
        public void CalculateTotalPrice_FourDays_5PercentOnDay4()
        {
            // Days 1-3: 3 × 100 = 300
            // Day 4: 100 × 0.95 = 95
            // Total = 395
            var result = CarController.CalculateTotalPrice(4, 100m);
            Assert.Equal(395m, result);
        }

        [Fact]
        public void CalculateTotalPrice_SixDays_IncrementalDiscounts()
        {
            // Days 1-3: 3 × 100 = 300
            // Day 4: 100 × 0.95 = 95  (5%)
            // Day 5: 100 × 0.90 = 90  (10%)
            // Day 6: 100 × 0.85 = 85  (15%)
            // Total = 570
            var result = CarController.CalculateTotalPrice(6, 100m);
            Assert.Equal(570m, result);
        }

        [Fact]
        public void CalculateTotalPrice_ElevenDays_CapsAt40Percent()
        {
            // Days 1-3: 300
            // Day 4: 95   (5%)
            // Day 5: 90   (10%)
            // Day 6: 85   (15%)
            // Day 7: 80   (20%)
            // Day 8: 75   (25%)
            // Day 9: 70   (30%)
            // Day 10: 65  (35%)
            // Day 11: 60  (40% — max)
            // Total = 920
            var result = CarController.CalculateTotalPrice(11, 100m);
            Assert.Equal(920m, result);
        }

        [Fact]
        public void CalculateTotalPrice_FifteenDays_DiscountStaysAt40()
        {
            // Days 1-3: 300
            // Day 4-10: 95+90+85+80+75+70+65 = 560
            // Days 11-15: 5 × 60 = 300 (all at 40%)
            // Total = 1160
            var result = CarController.CalculateTotalPrice(15, 100m);
            Assert.Equal(1160m, result);
        }

        [Fact]
        public void CalculateTotalPrice_ZeroDays_ReturnsZero()
        {
            var result = CarController.CalculateTotalPrice(0, 100m);
            Assert.Equal(0m, result);
        }

        [Fact]
        public void CalculateTotalPrice_RealWorldPrice_CorrectRounding()
        {
            // Price: €75/day, 5 days
            // Days 1-3: 225
            // Day 4: 75 × 0.95 = 71.25
            // Day 5: 75 × 0.90 = 67.50
            // Total = 363.75
            var result = CarController.CalculateTotalPrice(5, 75m);
            Assert.Equal(363.75m, result);
        }
    }

    public class EntityTests
    {
        [Fact]
        public void Car_DefaultValues_IsDeletedFalse()
        {
            var car = new Car();
            Assert.False(car.IsDeleted);
        }

        [Fact]
        public void RentalBooking_DefaultValues_Correct()
        {
            var booking = new RentalBooking();
            Assert.Equal("Active", booking.Status);
            Assert.True(booking.CreatedAt > DateTime.MinValue, "CreatedAt should default to DateTime.Now, not default(DateTime).");
        }

        [Fact]
        public void User_RequiredProperties_CanBeSet()
        {
            var user = new User
            {
                Username = "testuser",
                PasswordHash = "somehash",
                Role = "Client"
            };

            Assert.Equal("testuser", user.Username);
            Assert.Equal("Client", user.Role);
        }
    }

    public class DatabaseTests : IDisposable
    {
        private readonly ApplicationDbContext _context;

        public DatabaseTests()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            _context = new ApplicationDbContext(options);
            _context.Database.EnsureCreated();
        }

        public void Dispose()
        {
            _context.Database.EnsureDeleted();
            _context.Dispose();
        }

        [Fact]
        public void SeedData_CreatesAdminUser()
        {
            var admin = _context.Users.FirstOrDefault(u => u.Username == "admin");
            Assert.NotNull(admin);
            Assert.Equal("Admin", admin.Role);
        }

        [Fact]
        public void SeedData_CreatesSampleCars()
        {
            var cars = _context.Cars.ToList();
            Assert.True(cars.Count >= 4);
        }

        [Fact]
        public void GlobalQueryFilter_HidesDeletedCars()
        {
            var car = _context.Cars.First();
            car.IsDeleted = true;
            _context.SaveChanges();

            var visibleCars = _context.Cars.ToList();
            Assert.DoesNotContain(visibleCars, c => c.Id == car.Id);

            var allCars = _context.Cars.IgnoreQueryFilters().ToList();
            Assert.Contains(allCars, c => c.Id == car.Id);
        }

        [Fact]
        public async Task RentalBooking_CanBeCreatedAndQueried()
        {
            var booking = new RentalBooking
            {
                UserId = 2,
                CarId = 1,
                StartDate = DateTime.Today,
                EndDate = DateTime.Today.AddDays(3),
                TotalPrice = 225m,
                Status = "Active",
                CreatedAt = DateTime.Now
            };

            _context.RentalBookings.Add(booking);
            await _context.SaveChangesAsync();

            var saved = await _context.RentalBookings
                .FirstOrDefaultAsync(b => b.Id == booking.Id);

            Assert.NotNull(saved);
            Assert.Equal("Active", saved.Status);
            Assert.Equal(225m, saved.TotalPrice);
        }

        [Fact]
        public async Task UnavailabilityPeriod_CanBeCreated()
        {
            var period = new UnavailabilityPeriod
            {
                CarId = 1,
                StartDate = DateTime.Today,
                EndDate = DateTime.Today.AddDays(7),
                Reason = "Maintenance",
                CreatedAt = DateTime.Now
            };

            _context.UnavailabilityPeriods.Add(period);
            await _context.SaveChangesAsync();

            var saved = await _context.UnavailabilityPeriods
                .FirstOrDefaultAsync(u => u.Id == period.Id);

            Assert.NotNull(saved);
            Assert.Equal("Maintenance", saved.Reason);
        }

        [Fact]
        public void DatabaseSeeder_SeedsSampleData()
        {
            DatabaseSeeder.SeedSampleData(_context);

            var bookings = _context.RentalBookings.ToList();
            Assert.True(bookings.Count >= 3);

            // Verify CreatedAt is set (not default)
            foreach (var booking in bookings)
            {
                Assert.NotEqual(default, booking.CreatedAt);
                Assert.True(booking.CreatedAt.Year >= 2026);
            }
        }

        [Fact]
        public void DatabaseSeeder_DoesNotDuplicateOnMultipleCalls()
        {
            DatabaseSeeder.SeedSampleData(_context);
            int firstCount = _context.RentalBookings.Count();

            DatabaseSeeder.SeedSampleData(_context);
            int secondCount = _context.RentalBookings.Count();

            Assert.Equal(firstCount, secondCount);
        }
    }
}
