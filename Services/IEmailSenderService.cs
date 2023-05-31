using Microsoft.Extensions.Options;
using RazorLight;
using Starter_NET_7.Config;
using Starter_NET_7.Interfaces;
using System.Net;
using System.Net.Mail;

namespace Starter_NET_7.Services
{
    public class IEmailSenderService : IEmailSender
    {
        private readonly EmailSmtpSettings _emailSetting;
        private readonly AppSettings _appSettings;

        public IEmailSenderService(IOptions<EmailSmtpSettings> options, AppSettings appSettings)
        {
            _emailSetting = options.Value;
            _appSettings = appSettings;
        }

        public MailMessage CreateSmtpMail(string subject, IEnumerable<string> addresses, string body, bool isBodyHtml)
        {
            MailMessage mail = new MailMessage();
            mail.From = new MailAddress(_emailSetting.From);
            mail.Subject = subject;
            mail.Body = body;
            mail.IsBodyHtml = isBodyHtml;

            foreach (string address in addresses)
            {
                mail.To.Add(address);
            }

            return mail;
        }

        public MailMessage CreateSmtpMail(string subject, string address, string body, bool isBodyHtml)
        {
            MailMessage mail = new MailMessage();
            mail.From = new MailAddress(_emailSetting.From);
            mail.Subject = subject;
            mail.Body = body;
            mail.IsBodyHtml = isBodyHtml;
            mail.To.Add(address);

            return mail;
        }

        public bool SendSmtpMail(MailMessage mail)
        {
            try
            {
                var emailSmtp = new SmtpClient(_emailSetting.Host, _emailSetting.Port)
                {
                    Credentials = new NetworkCredential(_emailSetting.Username, _emailSetting.Password),
                    EnableSsl = _emailSetting.UseSSL
                };
                emailSmtp.Send(mail);
                emailSmtp.Dispose();
                return true;
            }
            catch
            {
                return false;
            }
        }
        
        public async Task<string> ViewToString<T>(string view, T model)
        {
            var engine = new RazorLightEngineBuilder()
                .UseFileSystemProject(_appSettings.PathViewEmail)
                .UseMemoryCachingProvider()
                .Build();

            return await engine.CompileRenderAsync($"{view}.cshtml", model);
        }
    }
}
