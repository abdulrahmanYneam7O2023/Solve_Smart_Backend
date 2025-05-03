using MimeKit;
using System.Net.Mail;
using System.Net;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;

namespace Solve_Smart_Backend.Service
{
    public class EmailSender : IEmailSender
    {
        private readonly IConfiguration _config;

        public EmailSender(IConfiguration config)
        {
            _config = config;
        }
     

        public async Task SendEmailAsync(string email, string subject, string htmlMessage)
        {

            var emailMessage = new MimeMessage();


            emailMessage.From.Add(new MailboxAddress(
                _config["Email:SenderName"],
                _config["Email:SenderEmail"]
            ));


            emailMessage.To.Add(new MailboxAddress(subject, email));


            emailMessage.Subject = subject;
            emailMessage.Body = new TextPart("html")
            {
                Text = $"<div><h1>{subject}</h1><h4>{email}</h4><p>{htmlMessage}</p></div>"
            };
            using var client = new MailKit.Net.Smtp.SmtpClient();


            await client.ConnectAsync(
                _config["Email:SmtpServer"],
                int.Parse(_config["Email:Port"]!),
                MailKit.Security.SecureSocketOptions.StartTls
            );

            await client.AuthenticateAsync(
                _config["Email:Username"],
                _config["Email:Password"]
            );

            await client.SendAsync(emailMessage);
            await client.DisconnectAsync(true);
        }
    }
}
