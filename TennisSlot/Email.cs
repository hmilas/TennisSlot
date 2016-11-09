using NLog;
using System;
using System.Configuration;
using System.Net;
using System.Net.Mail;

namespace TennisSlot
{
    public class Email
    {
        public static void Send(MailMessage mailMessage)
        {
            try
            {
                using (var client = new SmtpClient())
                {
                    client.Host = ConfigurationManager.AppSettings["MailHost"];
                    client.Port = 587;
                    client.UseDefaultCredentials = false;
                    client.DeliveryMethod = SmtpDeliveryMethod.Network;
                    client.EnableSsl = true;

                    client.Credentials = new NetworkCredential(
                        ConfigurationManager.AppSettings["MailFromAddresss"],
                        ConfigurationManager.AppSettings["MailPassword"]);

                    client.Send(mailMessage);

                    LogManager.GetCurrentClassLogger().Info("Mail sent To: " + mailMessage.To.ToString() + " Body: " + mailMessage.Body);
                }
            }
            catch (Exception ex)
            {
                LogManager.GetCurrentClassLogger().Info(ex.GetFormatted());
            }
        }
    }
}
