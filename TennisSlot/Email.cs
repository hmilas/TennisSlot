using System.Configuration;
using System.Net;
using System.Net.Mail;

namespace TennisSlot
{
    public class Email
    {
        public static void Send(MailMessage mailMessage)
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
            }
        }
    }
}
