using System;
using System.Web.Mail;

namespace SvnPostCommitHook
{
	public abstract class Mailer
	{
		private string _to;
		private string _from;
		private string _subject;
		private string _textbody;
		private string _htmlbody;
		private string _smtphost = "127.0.0.1";

		private string _smtpusername = "";
		private string _smtppassword = "";
		private string _smtpauthenticationtype = "";
		private int _smtpserverport = 25;


		public abstract void Send();
		
		public string To
		{
			get 
			{
				return _to;
			}
			set 
			{
				_to = value;
			}
		}

		public string From
		{
			get 
			{
				return _from;
			}
			set 
			{
				_from = value;
			}
		}

		public string Subject
		{
			get 
			{
				return _subject;
			}
			set 
			{
				_subject = value;
			}
		}

		public string TextBody
		{
			get 
			{
				return _textbody;
			}
			set 
			{
				_textbody = value;
			}
		}

		public string HtmlBody 
		{
			get 
			{
				return _htmlbody;
			}
			set 
			{
				_htmlbody = value;
			}
		}

		public string SmtpHost
		{
			get 
			{
				return _smtphost;
			}
			set 
			{
				_smtphost = value;
			}
		}

		public string SmtpAuthUsername
		{
			get 
			{
				return _smtpusername;
			}
			set 
			{
				_smtpusername = value;
			}
		}

		public string SmtpAuthPassword
		{
			get 
			{
				return _smtppassword;
			}
			set 
			{
				_smtppassword = value;
			}
		}

		public string SmtpAuthType
		{
			get 
			{
				return _smtpauthenticationtype;
			}
			set 
			{
				_smtpauthenticationtype = value;
			}
		}

		public int SmtpServerPort
		{
			get 
			{
				return _smtpserverport;
			}
			set 
			{
				_smtpserverport = value;
			}
		}
	}
}
