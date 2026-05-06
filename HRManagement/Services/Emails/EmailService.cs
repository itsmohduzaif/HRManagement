using HRManagement.Models;
using HRManagement.Models.Email;
using Microsoft.Extensions.Options;
using System.Net;
using System.Net.Mail;

namespace HRManagement.Services.Emails
{
    //public class EmailService : IEmailService
    public class EmailService : IEmailService
    {
        private readonly SmtpSettings _smtpSettings;

        public EmailService(IOptions<SmtpSettings> smtpSettings)
        {
            _smtpSettings = smtpSettings.Value;
        }

        public void SendEmail(string toEmail, string subject, string body)
        {
            var fromAddress = new MailAddress(_smtpSettings.FromEmail, _smtpSettings.FromName);
            var toAddress = new MailAddress(toEmail);
            
            var smtp = new SmtpClient
            {
                Host = _smtpSettings.Host,
                Port = _smtpSettings.Port,
                EnableSsl = _smtpSettings.EnableSsl,
                DeliveryMethod = SmtpDeliveryMethod.Network,
                UseDefaultCredentials = false,
                Credentials = new NetworkCredential(_smtpSettings.Username, _smtpSettings.Password)
            };

            using (var message = new MailMessage(fromAddress, toAddress)
            {
                Subject = subject,
                Body = body,
                IsBodyHtml = false
            })
            {
                smtp.Send(message);
            }
        }





        public async Task SendEmailAsync(EmailRequest request)
        {
            var fromAddress = new MailAddress(_smtpSettings.FromEmail, _smtpSettings.FromName);

            using var message = new MailMessage
            {
                From = fromAddress,
                Subject = request.Subject,
                Body = request.Body,
                IsBodyHtml = request.IsBodyHtml
            };

            foreach (var to in request.To.Where(x => !string.IsNullOrWhiteSpace(x)))
            {
                message.To.Add(new MailAddress(to));
            }

            foreach (var cc in request.Cc.Where(x => !string.IsNullOrWhiteSpace(x)))
            {
                message.CC.Add(new MailAddress(cc));
            }

            var attachmentStreams = new List<MemoryStream>();

            try
            {
                foreach (var file in request.Attachments.Where(f => f != null && f.Length > 0))
                {
                    var stream = new MemoryStream();
                    await file.CopyToAsync(stream);
                    stream.Position = 0;

                    attachmentStreams.Add(stream);

                    var attachment = new Attachment(stream, file.FileName, file.ContentType);
                    message.Attachments.Add(attachment);
                }
                    
                using var smtp = new SmtpClient
                {
                    Host = _smtpSettings.Host,
                    Port = _smtpSettings.Port,
                    EnableSsl = _smtpSettings.EnableSsl,
                    DeliveryMethod = SmtpDeliveryMethod.Network,
                    UseDefaultCredentials = false,
                    Credentials = new NetworkCredential(_smtpSettings.Username, _smtpSettings.Password)
                };

                await smtp.SendMailAsync(message);
            }
            finally
            {
                foreach (var stream in attachmentStreams)
                {
                    stream.Dispose();
                }
            }
        }





    }
}
