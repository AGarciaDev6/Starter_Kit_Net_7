using System.Net.Mail;

namespace Starter_NET_7.Interfaces
{
    public interface IEmailSender
    {
        public MailMessage CreateSmtpMail(string subject, IEnumerable<string> addresses, string body, bool isBodyHtml);
        public MailMessage CreateSmtpMail(string subject, string address, string body, bool isBodyHtml);
        public bool SendSmtpMail(MailMessage mail);
        public Task<string> ViewToString<T>(string path, T model);
    }
}
