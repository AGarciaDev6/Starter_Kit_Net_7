using Microsoft.Extensions.Options;
using Starter_NET_7.AppSettings;
using Starter_NET_7.Interfaces;
using System.Net;
using System.Net.Mail;

namespace Starter_NET_7.Services
{
  public class IEmailSenderService : IEmailSender
  {
    private readonly EmailSmtpSettings _emailSetting;

    public IEmailSenderService(IOptions<EmailSmtpSettings> options)
    {
      this._emailSetting = options.Value;
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
  }
}
