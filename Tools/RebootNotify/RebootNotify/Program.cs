using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Mail;

namespace RebootNotify
{
    class Program
    {
        static void Main(string[] args)
        {
            SendMail();
        }

        private static void SendMail()
        {
            MailMessage mail = new MailMessage();

            mail.From = new MailAddress(GetFromAddress());
            mail.To.Add(GetToAddress());

            mail.Subject = "Reboot Server: " + Environment.MachineName;
            mail.Body = "Date/Time: " + DateTime.Now.ToString();

            SmtpClient smtp = new SmtpClient("127.0.0.1");
            smtp.Send(mail);
        }

        private static string GetToAddress()
        {
            return "wil@emailgwiax.com";
        }

        private static string GetFromAddress()
        {
            return "christophw@alphasierrapapa.com";
        }
    }
}
