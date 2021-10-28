using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.Extensions.Options;
using MailKit.Net.Smtp;
using MimeKit;

namespace MTF.Services
{
    // Parameters from any configuration source
    public class IdentityMailSenderOptions
    {
        public string Server { get; set; }
        public string Port { get; set; }
        public string User { get; set; }
        public string Password { get; set; }
        public string SenderName { get; set; }
        public string SenderEmail { get; set; }
    }
 
    // Interface class to send Email from Identity component
    public class IdentityEmailSenderTrivial : IEmailSender
    {
        private IdentityMailSenderOptions _options {get; set;}
        private int _port { get; set; }

        public IdentityEmailSenderTrivial (IOptions<IdentityMailSenderOptions> options)
        {
            _options = options.Value;
            _port = Convert.ToInt32(_options.Port);
        }

        public async Task SendEmailAsync(string address, string subject, string body)
        {
            MimeMessage message = new MimeMessage();

            MailboxAddress from = new MailboxAddress(_options.SenderName,
                                                     _options.SenderEmail
                                                    );
            message.From.Add(from);
            MailboxAddress to = new MailboxAddress(address,
                                                   address
                                                  );
            message.To.Add(to);
            message.Subject = subject;

            BodyBuilder bodyBuilder = new BodyBuilder();
            bodyBuilder.HtmlBody = body;
            message.Body = bodyBuilder.ToMessageBody();

            using SmtpClient client = new SmtpClient();
            try
            {
                await client.ConnectAsync(_options.Server, _port, MailKit.Security.SecureSocketOptions.StartTlsWhenAvailable);
                await client.AuthenticateAsync(_options.User, _options.Password);

                await client.SendAsync(message);
            }
            catch
            {
                throw;
            }
            finally
            {
                await client.DisconnectAsync(true);
                client.Dispose();
            }
        }
    }
}
