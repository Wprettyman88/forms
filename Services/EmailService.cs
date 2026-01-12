using System.Net;
using System.Net.Mail;
using Microsoft.Extensions.Configuration;

namespace WiseLabels.Services
{
    public class EmailService : IEmailService
    {
        private readonly ILogger<EmailService> _logger;
        private readonly IConfiguration _configuration;

        public EmailService(ILogger<EmailService> logger, IConfiguration configuration)
        {
            _logger = logger;
            _configuration = configuration;
        }

        public async Task<bool> SendQuoteConfirmationAsync(string toEmail, string quoteId, string customerName)
        {
            try
            {
                var smtpHost = _configuration["Email:SmtpHost"];
                var smtpPort = _configuration.GetValue<int>("Email:SmtpPort", 587);
                var smtpUsername = _configuration["Email:SmtpUsername"];
                var smtpPassword = _configuration["Email:SmtpPassword"];
                var fromEmail = _configuration["Email:FromEmail"] ?? "noreply@wiselinklabels.com";
                var fromName = _configuration["Email:FromName"] ?? "WiseLink Labels";

                if (string.IsNullOrEmpty(smtpHost) || string.IsNullOrEmpty(smtpUsername) || string.IsNullOrEmpty(smtpPassword))
                {
                    _logger.LogWarning("Email configuration is incomplete. Email will not be sent.");
                    return false;
                }

                using var client = new SmtpClient(smtpHost, smtpPort)
                {
                    EnableSsl = true,
                    Credentials = new NetworkCredential(smtpUsername, smtpPassword)
                };

                var message = new MailMessage
                {
                    From = new MailAddress(fromEmail, fromName),
                    Subject = $"Quote Request Confirmation - #{quoteId}",
                    Body = GenerateEmailBody(quoteId, customerName),
                    IsBodyHtml = true
                };

                message.To.Add(toEmail);

                await client.SendMailAsync(message);
                _logger.LogInformation("Confirmation email sent successfully to {Email} for quote {QuoteId}", toEmail, quoteId);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending confirmation email to {Email} for quote {QuoteId}", toEmail, quoteId);
                return false;
            }
        }

        private string GenerateEmailBody(string quoteId, string customerName)
        {
            var greeting = !string.IsNullOrWhiteSpace(customerName) ? $"Hello {customerName}," : "Hello,";
            
            return $@"
<!DOCTYPE html>
<html>
<head>
    <meta charset=""utf-8"">
    <style>
        body {{ font-family: Arial, sans-serif; line-height: 1.6; color: #333; }}
        .container {{ max-width: 600px; margin: 0 auto; padding: 20px; }}
        .header {{ background-color: #007bff; color: white; padding: 20px; text-align: center; }}
        .content {{ background-color: #f9f9f9; padding: 20px; }}
        .quote-id {{ background-color: #fff; padding: 15px; margin: 20px 0; border-left: 4px solid #007bff; }}
        .footer {{ text-align: center; padding: 20px; font-size: 12px; color: #666; }}
    </style>
</head>
<body>
    <div class=""container"">
        <div class=""header"">
            <h1>Thank You for Your Quote Request</h1>
        </div>
        <div class=""content"">
            <p>{greeting}</p>
            <p>Thank you for requesting a quote from WiseLink Labels. We have successfully received your quote request and our team will review it shortly.</p>
            <div class=""quote-id"">
                <strong>Your Quote Reference:</strong><br>
                <span style=""font-size: 24px; font-weight: bold; color: #007bff;"">#{quoteId}</span>
            </div>
            <p>Please save this reference number for your records. We will contact you soon with pricing and additional details.</p>
            <p>If you have any questions, please don't hesitate to contact us.</p>
            <p>Best regards,<br>WiseLink Labels Team</p>
        </div>
        <div class=""footer"">
            <p>This is an automated confirmation email. Please do not reply to this message.</p>
        </div>
    </div>
</body>
</html>";
        }
    }
}

