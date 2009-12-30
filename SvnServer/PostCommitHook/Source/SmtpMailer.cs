using System;
using DotNetOpenMail;
using DotNetOpenMail.SmtpAuth;

namespace SvnPostCommitHook
{
	/// <summary>
	/// Summary description for SmtpMailer.
	/// </summary>
	public class SmtpMailer : Mailer
	{

		private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

		public override void Send()
		{
			EmailMessage msg = new EmailMessage();
			foreach(string addr in AddressesFromList(To))
				msg.AddToAddress(addr);

			msg.FromAddress = new EmailAddress(From);
			msg.Subject = Subject;
			if(TextBody != null) msg.TextPart = new TextAttachment(TextBody);
			if(HtmlBody != null) msg.HtmlPart = new HtmlAttachment(HtmlBody);
			
			SmtpServer server = new SmtpServer(SmtpHost, SmtpServerPort);

			switch (SmtpAuthType)
			{
				case "BASIC":
					log.Info("BASIC authentication was added to SMTP request");
					SmtpAuthToken at = new SmtpAuthToken(SmtpAuthUsername, SmtpAuthPassword);
					server.SmtpAuthToken = at;
					break;
				default:
					// no authentication
					break;
			}

			log.Info(string.Format("Sending mail to {0} from {1} with subject {2}", msg.ToAddresses, msg.FromAddress, msg.Subject));
			msg.Send(server);
			log.Info("Sent mail");
		}

		string[] AddressesFromList(string list) 
		{
			return list.Split(';');
		}

	}
}
