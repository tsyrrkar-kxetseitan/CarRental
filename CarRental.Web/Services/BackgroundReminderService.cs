using CarRental.Data;
using Microsoft.EntityFrameworkCore;

namespace CarRental.Web.Services
{
    /// <summary>
    /// Background service that runs every 6 hours and sends rental reminder
    /// emails + in-site notifications for bookings starting in 3 days.
    /// </summary>
    public class BackgroundReminderService : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<BackgroundReminderService> _logger;

        public BackgroundReminderService(IServiceScopeFactory scopeFactory, ILogger<BackgroundReminderService> logger)
        {
            _scopeFactory = scopeFactory;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("BackgroundReminderService started.");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await SendRemindersAsync();
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error in BackgroundReminderService.");
                }

                // Run every 6 hours
                await Task.Delay(TimeSpan.FromHours(6), stoppingToken);
            }
        }

        private async Task SendRemindersAsync()
        {
            using var scope = _scopeFactory.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var emailService = scope.ServiceProvider.GetRequiredService<IEmailService>();
            var notificationService = scope.ServiceProvider.GetRequiredService<INotificationService>();

            var reminderDate = DateTime.Today.AddDays(3);

            var upcomingBookings = await context.RentalBookings
                .Include(b => b.User)
                .Include(b => b.Car)
                .Where(b => b.Status == "Active"
                    && b.StartDate == reminderDate)
                .ToListAsync();

            _logger.LogInformation("Found {Count} bookings starting in 3 days.", upcomingBookings.Count);

            foreach (var booking in upcomingBookings)
            {
                if (booking.User == null || booking.Car == null) continue;

                var carName = $"{booking.Car.Brand} {booking.Car.Model}";

                try
                {
                    await emailService.SendRentalReminderAsync(
                        booking.User.Email, booking.User.Username,
                        carName, booking.StartDate, booking.EndDate);

                    await notificationService.CreateNotificationAsync(
                        booking.UserId,
                        "Rental Reminder",
                        $"Your rental of {carName} starts on {booking.StartDate:dd MMM yyyy}. Don't forget your driver's license!",
                        "Reminder");

                    _logger.LogInformation("Sent reminder for booking #{Id} to {User}", booking.Id, booking.User.Username);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to send reminder for booking #{Id}", booking.Id);
                }
            }
        }
    }
}
