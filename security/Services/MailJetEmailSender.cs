using Microsoft.AspNetCore.Identity.UI.Services;
using System.Net;
using System.Net.Mail;

namespace security.Services
{
    public class MailJetEmailSender : IEmailSender
    {
        private readonly IConfiguration _configuration;

        public MailJetEmailSender(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public async Task SendEmailAsync(string email, string subject, string htmlMessage)
        {
            var message = new MailMessage
            {
                From = new MailAddress(_configuration["Email:From"]),
                Subject = subject,
                Body = htmlMessage,                
                IsBodyHtml = true
            };
            message.To.Add(email);
            var client = new SmtpClient
            {
                Host = _configuration["Email:Smtp"],
                Port = int.Parse(_configuration["Email:Port"]),
                EnableSsl = true,
                Credentials = new NetworkCredential(
             _configuration["Email:Username"],
             _configuration["Email:Password"]
         )
            };
            await client.SendMailAsync(message);
        }
    }
}
