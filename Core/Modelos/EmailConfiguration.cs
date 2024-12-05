using Core.DTOs;
using Core.Modelos.Common;
using System.Net;
using System.Net.Mail;

namespace Core.Modelos
{
    public class EmailConfiguration : BaseEntity
    {
        public string SmtpServer { get; set; }
        public int Port { get; set; }
        public bool EnableSsl { get; set; }
        public string UserName { get; set; }
        public string Password { get; set; }

        public EmailConfiguration(string smtpServer, int port, bool enableSsl, string userName, string password)
        {
            SmtpServer = smtpServer;
            Port = port;
            EnableSsl = enableSsl;
            UserName = userName;
            Password = password;
        }

        public void SendEmail(string fromEmail, string toEmail, string subject, string body, string attachmentPath)
        {
            var smtpClient = new SmtpClient(SmtpServer)
            {
                Port = Port,
                Credentials = new NetworkCredential(UserName, Password),
                EnableSsl = EnableSsl,
            };

            var mailMessage = new MailMessage
            {
                From = new MailAddress(fromEmail),
                Subject = subject,
                Body = body,
                IsBodyHtml = true,
            };

            mailMessage.To.Add(toEmail);

            if (!string.IsNullOrEmpty(attachmentPath))
            {
                Attachment attachment = new(attachmentPath);
                mailMessage.Attachments.Add(attachment);
            }

            smtpClient.Send(mailMessage);
        }

        public void SendEmail(string[] toEmail, string[]? toCC, string[]? toCCO, string subject, string body, AttachmentFileDto[]? attachment)
        {
            try
            {
                using MailMessage email = new()
                {
                    From = new(UserName),
                    Subject = subject,
                    Body = body,
                    IsBodyHtml = true,
                };

                foreach (var item in toEmail)
                    email.To.Add(item);

                if (toCC != null)
                    foreach (var item in toCC)
                        email.CC.Add(item);

                if (toCCO != null)
                    foreach (var item in toCCO)
                        email.Bcc.Add(item);

                if (attachment != null && attachment.Length != 0)
                    foreach (var item in attachment)
                    {
                        var file = item.File;
                        byte[] bytes = file.ToArray();
                        file.Close();
                        email.Attachments.Add(new Attachment(new MemoryStream(bytes), $"{item.FileName}.{item.FileExtension}"));
                    }

                using var smtpClient = new SmtpClient(SmtpServer)
                {
                    Port = Port,
                    Credentials = new NetworkCredential(UserName, Password),
                    EnableSsl = EnableSsl,
                };

                smtpClient.Send(email);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

    }
}
