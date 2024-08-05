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

        public void SendEmail(string fromEmail, string toEmail, string subject, string body, string attachmentPath = null)
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
    }
}
