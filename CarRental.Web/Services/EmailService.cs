using System.Net;
using System.Net.Mail;

namespace CarRental.Web.Services
{
    public class BookingEmailInfo
    {
        public string CarName { get; set; } = "";
        public string LicensePlate { get; set; } = "";
        public string Location { get; set; } = "";
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public decimal TotalPrice { get; set; }
    }

    public interface IEmailService
    {
        Task SendWelcomeEmailAsync(string toEmail, string username);
        Task SendBookingConfirmationAsync(string toEmail, string username, List<BookingEmailInfo> bookings, decimal totalPaid);
        Task SendCancellationByAdminAsync(string toEmail, string username, string carName, DateTime startDate, DateTime endDate, decimal refundAmount);
        Task SendRentalReminderAsync(string toEmail, string username, string carName, DateTime startDate, DateTime endDate);
    }

    public class EmailService : IEmailService
    {
        private readonly IConfiguration _config;
        private readonly ILogger<EmailService> _logger;

        public EmailService(IConfiguration config, ILogger<EmailService> logger)
        {
            _config = config;
            _logger = logger;
        }

        private async Task SendEmailAsync(string to, string subject, string htmlBody)
        {
            try
            {
                var smtpServer = _config["EmailSettings:SmtpServer"] ?? "smtp.gmail.com";
                var smtpPort = int.Parse(_config["EmailSettings:SmtpPort"] ?? "587");
                var senderEmail = _config["EmailSettings:SenderEmail"] ?? "roadrunnerssrl@gmail.com";
                var senderName = _config["EmailSettings:SenderName"] ?? "RoadRunners Car Rental";
                var appPassword = _config["EmailSettings:AppPassword"] ?? "";

                using var client = new SmtpClient(smtpServer, smtpPort)
                {
                    Credentials = new NetworkCredential(senderEmail, appPassword),
                    EnableSsl = true
                };

                var message = new MailMessage
                {
                    From = new MailAddress(senderEmail, senderName),
                    Subject = subject,
                    Body = htmlBody,
                    IsBodyHtml = true
                };
                message.To.Add(to);

                await client.SendMailAsync(message);
                _logger.LogInformation("Email sent to {Email}: {Subject}", to, subject);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send email to {Email}: {Subject}", to, subject);
            }
        }

        private static string WrapInTemplate(string content)
        {
            return $@"
<!DOCTYPE html>
<html>
<head><meta charset='utf-8'></head>
<body style='margin:0;padding:0;background:#f4f4f4;font-family:Arial,Helvetica,sans-serif;'>
<table width='100%' cellpadding='0' cellspacing='0' style='background:#f4f4f4;padding:20px 0;'>
<tr><td align='center'>
<table width='600' cellpadding='0' cellspacing='0' style='background:#ffffff;border-radius:8px;overflow:hidden;box-shadow:0 2px 8px rgba(0,0,0,0.1);'>
<tr>
<td style='background:linear-gradient(135deg,#1a1a2e,#16213e);padding:25px 30px;text-align:center;'>
<h1 style='margin:0;color:#ff6b35;font-size:28px;letter-spacing:1px;'>RoadRunners</h1>
<p style='margin:5px 0 0;color:#aaa;font-size:13px;'>Car Rental Service</p>
</td>
</tr>
<tr>
<td style='padding:30px;'>
{content}
</td>
</tr>
<tr>
<td style='background:#1a1a2e;padding:20px 30px;text-align:center;'>
<p style='margin:0;color:#888;font-size:12px;'>RoadRunners Car Rental SRL</p>
<p style='margin:4px 0 0;color:#666;font-size:11px;'>Observatorului 26-28, Cluj-Napoca, Romania</p>
<p style='margin:4px 0 0;color:#666;font-size:11px;'>roadrunnerssrl@gmail.com | +40 700 123 456</p>
</td>
</tr>
</table>
</td></tr>
</table>
</body>
</html>";
        }

        public async Task SendWelcomeEmailAsync(string toEmail, string username)
        {
            var content = $@"
<h2 style='color:#1a1a2e;margin-top:0;'>Welcome aboard, {username}!</h2>
<p style='color:#333;font-size:15px;line-height:1.6;'>
Thank you for joining <strong style='color:#ff6b35;'>RoadRunners Car Rental</strong>! We're thrilled to have you as part of our community.
</p>
<div style='background:#f0fff4;border-left:4px solid #38a169;padding:15px;border-radius:4px;margin:20px 0;'>
<p style='margin:0;color:#2d6a4f;font-size:15px;'>
<strong>EUR 20.00 Welcome Bonus</strong> has been credited to your account balance!
</p>
<p style='margin:5px 0 0;color:#555;font-size:13px;'>Use it towards your first rental booking.</p>
</div>
<p style='color:#333;font-size:15px;line-height:1.6;'>
Start exploring our premium fleet of vehicles across Romania's major cities. From compact city cars to luxury SUVs, we have something for every journey.
</p>
<div style='text-align:center;margin:25px 0;'>
<a href='http://localhost:5103/Car' style='background:#ff6b35;color:#fff;padding:12px 30px;text-decoration:none;border-radius:6px;font-weight:bold;font-size:15px;'>Browse Our Fleet</a>
</div>
<p style='color:#888;font-size:13px;'>Happy driving!<br/>The RoadRunners Team</p>";

            await SendEmailAsync(toEmail, "Welcome to RoadRunners Car Rental!", WrapInTemplate(content));
        }

        public async Task SendBookingConfirmationAsync(string toEmail, string username, List<BookingEmailInfo> bookings, decimal totalPaid)
        {
            var rows = string.Join("", bookings.Select(b => $@"
<tr>
<td style='padding:10px;border-bottom:1px solid #eee;'>{b.CarName}</td>
<td style='padding:10px;border-bottom:1px solid #eee;'>{b.LicensePlate}</td>
<td style='padding:10px;border-bottom:1px solid #eee;'>{b.Location}</td>
<td style='padding:10px;border-bottom:1px solid #eee;'>{b.StartDate:dd MMM} - {b.EndDate:dd MMM yyyy}</td>
<td style='padding:10px;border-bottom:1px solid #eee;font-weight:bold;'>EUR {b.TotalPrice:F2}</td>
</tr>"));

            var content = $@"
<h2 style='color:#1a1a2e;margin-top:0;'>Booking Confirmed!</h2>
<p style='color:#333;font-size:15px;'>Hi <strong>{username}</strong>, your booking has been confirmed. Here are the details:</p>

<table width='100%' cellpadding='0' cellspacing='0' style='border:1px solid #eee;border-radius:6px;margin:15px 0;font-size:13px;'>
<thead>
<tr style='background:#1a1a2e;color:#fff;'>
<th style='padding:10px;text-align:left;'>Car</th>
<th style='padding:10px;text-align:left;'>Plate</th>
<th style='padding:10px;text-align:left;'>Location</th>
<th style='padding:10px;text-align:left;'>Period</th>
<th style='padding:10px;text-align:left;'>Price</th>
</tr>
</thead>
<tbody>
{rows}
</tbody>
</table>

<div style='background:#fff8f0;border-left:4px solid #ff6b35;padding:15px;border-radius:4px;margin:20px 0;'>
<p style='margin:0;font-size:16px;color:#1a1a2e;'>
Total Charged: <strong style='color:#ff6b35;'>EUR {totalPaid:F2}</strong>
</p>
</div>

<p style='color:#333;font-size:14px;line-height:1.6;'>
<strong>Pick-up reminder:</strong> Please arrive at the pick-up location on the start date with a valid driver's license and ID.
</p>
<p style='color:#888;font-size:13px;'>Thank you for choosing RoadRunners!</p>";

            await SendEmailAsync(toEmail, "Booking Confirmed - RoadRunners", WrapInTemplate(content));
        }

        public async Task SendCancellationByAdminAsync(string toEmail, string username, string carName, DateTime startDate, DateTime endDate, decimal refundAmount)
        {
            var content = $@"
<h2 style='color:#1a1a2e;margin-top:0;'>Booking Canceled</h2>
<p style='color:#333;font-size:15px;'>Hi <strong>{username}</strong>,</p>
<p style='color:#333;font-size:15px;line-height:1.6;'>
We regret to inform you that your booking has been canceled by our team. We sincerely apologize for the inconvenience.
</p>

<div style='background:#fff5f5;border-left:4px solid #e53e3e;padding:15px;border-radius:4px;margin:20px 0;'>
<p style='margin:0;color:#333;font-size:14px;'>
<strong>Car:</strong> {carName}<br/>
<strong>Period:</strong> {startDate:dd MMM yyyy} - {endDate:dd MMM yyyy}
</p>
</div>

<div style='background:#f0fff4;border-left:4px solid #38a169;padding:15px;border-radius:4px;margin:20px 0;'>
<p style='margin:0;color:#2d6a4f;font-size:15px;'>
<strong>EUR {refundAmount:F2} (120% refund)</strong> has been credited to your account balance as compensation.
</p>
</div>

<p style='color:#333;font-size:14px;line-height:1.6;'>
We encourage you to browse our fleet and book an alternative vehicle. Your refund is ready to use immediately.
</p>
<div style='text-align:center;margin:25px 0;'>
<a href='http://localhost:5103/Car' style='background:#ff6b35;color:#fff;padding:12px 30px;text-decoration:none;border-radius:6px;font-weight:bold;font-size:15px;'>Browse Available Cars</a>
</div>
<p style='color:#888;font-size:13px;'>We apologize again and thank you for your understanding.<br/>The RoadRunners Team</p>";

            await SendEmailAsync(toEmail, "Booking Canceled - RoadRunners", WrapInTemplate(content));
        }

        public async Task SendRentalReminderAsync(string toEmail, string username, string carName, DateTime startDate, DateTime endDate)
        {
            var content = $@"
<h2 style='color:#1a1a2e;margin-top:0;'>Your Rental Starts Soon!</h2>
<p style='color:#333;font-size:15px;'>Hi <strong>{username}</strong>,</p>
<p style='color:#333;font-size:15px;line-height:1.6;'>
This is a friendly reminder that your upcoming car rental is just <strong style='color:#ff6b35;'>3 days away</strong>!
</p>

<div style='background:#eff6ff;border-left:4px solid #3b82f6;padding:15px;border-radius:4px;margin:20px 0;'>
<p style='margin:0;color:#333;font-size:14px;'>
<strong>Car:</strong> {carName}<br/>
<strong>Period:</strong> {startDate:dd MMM yyyy} - {endDate:dd MMM yyyy}
</p>
</div>

<h3 style='color:#1a1a2e;'>Don't forget to bring:</h3>
<ul style='color:#333;font-size:14px;line-height:1.8;'>
<li>Valid driver's license</li>
<li>Government-issued ID or passport</li>
<li>Booking confirmation (this email)</li>
</ul>

<p style='color:#333;font-size:14px;line-height:1.6;'>
If you need to make any changes to your booking, please visit your account page.
</p>
<p style='color:#888;font-size:13px;'>See you soon!<br/>The RoadRunners Team</p>";

            await SendEmailAsync(toEmail, "Your Rental Starts Soon! - RoadRunners", WrapInTemplate(content));
        }
    }
}
