using System.Threading.Tasks;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using MimeKit;

namespace FinalMarzo.net.Services
{
    public class EmailService
    {
        private readonly string _smtpHost;
        private readonly int _smtpPort;
        private readonly string _smtpUser;
        private readonly string _smtpPass;
        private readonly ILogger<EmailService> _logger;

        public EmailService(IConfiguration configuration, ILogger<EmailService> logger)
        {
            _smtpHost =
                configuration["SMTP_Host"]
                ?? throw new System.Exception("SMTP_Host no configurado.");
            _smtpPort = int.Parse(configuration["SMTP_Port"] ?? "587");
            _smtpUser =
                configuration["SMTP_User"]
                ?? throw new System.Exception("SMTP_User no configurado.");
            _smtpPass =
                configuration["SMTP_Pass"]
                ?? throw new System.Exception("SMTP_Pass no configurado.");
            _logger = logger;
        }

        public async Task SendEmailAsync(string to, string subject, string body)
        {
            var message = new MimeMessage();
            message.From.Add(new MailboxAddress("Estacionamiento", _smtpUser));
            message.To.Add(new MailboxAddress("", to));
            message.Subject = subject;
            message.Body = new TextPart("plain") { Text = body };

            try
            {
                using var client = new SmtpClient();
                await client.ConnectAsync(_smtpHost, _smtpPort, SecureSocketOptions.StartTls);
                await client.AuthenticateAsync(_smtpUser, _smtpPass);
                await client.SendAsync(message);
                await client.DisconnectAsync(true);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error sending email: {ex}");
                throw;
            }
        }
    }
}
