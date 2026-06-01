using CarRental.Core.Entities;
using Microsoft.EntityFrameworkCore;

namespace CarRental.Data
{
    /// <summary>
    /// Seeds the database with sample runtime data (bookings, unavailability)
    /// using current dates. Call from Program.cs after Migrate().
    /// </summary>
    public static class DatabaseSeeder
    {
        public static void SeedSampleData(ApplicationDbContext context)
        {
            // Only seed if no bookings exist yet
            if (context.RentalBookings.IgnoreQueryFilters().Any())
                return;

            var today = DateTime.Today;
            var now = DateTime.Now;

            // Ensure we have a client user (Id=2)
            var client = context.Users.FirstOrDefault(u => u.Id == 2);
            if (client == null) return;

            // Sample bookings with current dates across various cars
            var bookings = new List<RentalBooking>
            {
                // Past completed booking — BMW Series 3 in București
                new RentalBooking
                {
                    UserId = 2, CarId = 1,
                    StartDate = today.AddDays(-14),
                    EndDate = today.AddDays(-10),
                    TotalPrice = CalculatePrice(4, 75.00m),
                    Status = "Active",
                    CreatedAt = now.AddDays(-15)
                },
                // Currently active booking — Mercedes C-Class in București
                new RentalBooking
                {
                    UserId = 2, CarId = 6,
                    StartDate = today.AddDays(-1),
                    EndDate = today.AddDays(3),
                    TotalPrice = CalculatePrice(4, 85.00m),
                    Status = "Active",
                    CreatedAt = now.AddDays(-3)
                },
                // Upcoming booking — Audi A4 in Brașov
                new RentalBooking
                {
                    UserId = 2, CarId = 12,
                    StartDate = today.AddDays(5),
                    EndDate = today.AddDays(12),
                    TotalPrice = CalculatePrice(7, 70.00m),
                    Status = "Active",
                    CreatedAt = now.AddDays(-1)
                },
                // Canceled booking — Toyota Corolla in București
                new RentalBooking
                {
                    UserId = 2, CarId = 16,
                    StartDate = today.AddDays(-7),
                    EndDate = today.AddDays(-4),
                    TotalPrice = CalculatePrice(3, 45.00m),
                    Status = "Canceled",
                    CreatedAt = now.AddDays(-10)
                },
                // Past booking — Dacia Duster in Brașov
                new RentalBooking
                {
                    UserId = 2, CarId = 29,
                    StartDate = today.AddDays(-21),
                    EndDate = today.AddDays(-18),
                    TotalPrice = CalculatePrice(3, 40.00m),
                    Status = "Active",
                    CreatedAt = now.AddDays(-25)
                },
                // Upcoming — Volkswagen Golf in București
                new RentalBooking
                {
                    UserId = 2, CarId = 21,
                    StartDate = today.AddDays(10),
                    EndDate = today.AddDays(15),
                    TotalPrice = CalculatePrice(5, 50.00m),
                    Status = "Active",
                    CreatedAt = now.AddDays(-2)
                }
            };

            context.RentalBookings.AddRange(bookings);

            // Sample unavailability periods
            var unavailPeriods = new List<UnavailabilityPeriod>
            {
                new UnavailabilityPeriod
                {
                    CarId = 3, // BMW X5 in București
                    StartDate = today.AddDays(1),
                    EndDate = today.AddDays(8),
                    Reason = "Scheduled maintenance",
                    CreatedAt = now.AddDays(-2)
                },
                new UnavailabilityPeriod
                {
                    CarId = 34, // Ford Mustang in București
                    StartDate = today.AddDays(3),
                    EndDate = today.AddDays(10),
                    Reason = "Bodywork repair",
                    CreatedAt = now.AddDays(-1)
                },
                new UnavailabilityPeriod
                {
                    CarId = 48, // Porsche 911 in București
                    StartDate = today.AddDays(-3),
                    EndDate = today.AddDays(5),
                    Reason = "Annual inspection",
                    CreatedAt = now.AddDays(-5)
                }
            };

            context.UnavailabilityPeriods.AddRange(unavailPeriods);
            context.SaveChanges();
        }

        private static decimal CalculatePrice(int totalDays, decimal pricePerDay)
        {
            decimal total = 0;
            for (int day = 1; day <= totalDays; day++)
            {
                if (day <= 3)
                    total += pricePerDay;
                else
                {
                    int discountSteps = day - 3;
                    decimal discountPercent = Math.Min(discountSteps * 5m, 40m);
                    total += pricePerDay * (1 - discountPercent / 100m);
                }
            }
            return Math.Round(total, 2);
        }
    }
}
