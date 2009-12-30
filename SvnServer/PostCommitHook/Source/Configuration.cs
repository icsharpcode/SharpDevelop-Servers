using System;
using System.Configuration;

namespace SvnPostCommitHook
{
	public sealed class TypedConfiguration
	{
		static TypedConfiguration instance=null;

		private string cssFile = "";

		private TypedConfiguration()
		{
		}

		public static TypedConfiguration Instance
		{
			get
			{
				if (instance==null)
				{
					instance = new TypedConfiguration();
				}
				return instance;
			}
		}

		public string SvnLookPath
		{
			get { return ConfigurationManager.AppSettings["SvnLookPath"]; }
		}

		public string CssForHtmlMail 
		{
			get 
			{
				// if we already figured this out, don't do it again
				if(cssFile != "") return cssFile;

				// did they specify a name in the config file?
				cssFile = ConfigurationManager.AppSettings["CssForHtmlMail"];

				// if nothing in config, default to HtmlMail.css
				if(cssFile == null || cssFile.Length == 0) cssFile = "HtmlMail.css";

				// if the file is found, return it, otherwise keep looking
				if(System.IO.File.Exists(cssFile)) return cssFile;

				// if the the path given is rooted, we can't probe any further
				if(!System.IO.Path.IsPathRooted(cssFile)) 
				{
					// otherwise, take a look relative to the probing directory.
					cssFile = System.IO.Path.Combine(System.AppDomain.CurrentDomain.BaseDirectory, cssFile);
					if(System.IO.File.Exists(cssFile)) return cssFile;
				}
				
				throw new System.IO.FileNotFoundException("Could not find CssForHtmlMail file", cssFile);

			}
				
		}

		public string MailTo
		{
			get { return ConfigurationManager.AppSettings["MailTo"]; }
		}

		public string MailFrom
		{
			get { return ConfigurationManager.AppSettings["MailFrom"]; }
		}

		public string MailSubject
		{
			get { return ConfigurationManager.AppSettings["MailSubject"]; }
		}

		public string MailServer
		{
			get { return ConfigurationManager.AppSettings["MailServer"]; }
		}

		public string SmtpAuthType
		{
			get { return ConfigurationManager.AppSettings["SMTPAuthentication"]; }
		}

		public string SmtpAuthUsername
		{
			get { return ConfigurationManager.AppSettings["SMTPUsername"]; }
		}

		public string SmtpAuthPassword
		{
			get { return ConfigurationManager.AppSettings["SMTPPassword"]; }
		}

		public int SmtpServerPort
		{
			get { return int.Parse(ConfigurationManager.AppSettings["SMTPServerPort"]); }
		}

		public bool AppendDiffToMail
		{
			get { return bool.Parse(ConfigurationManager.AppSettings["AppendDiffToMail"]); }
		}

		public bool MailTextOnly
		{
			get { return bool.Parse(ConfigurationManager.AppSettings["MailTextOnly"]); }
		}


		public Mailer Mailer 
		{
			get { 
				string mailerToUse = ConfigurationManager.AppSettings["Mailer"]; 
				if(mailerToUse == null) mailerToUse = String.Empty;
				mailerToUse = mailerToUse.ToLower(System.Globalization.CultureInfo.InvariantCulture);

				switch(mailerToUse) 
				{
					default:
						return new SmtpMailer();
				}
			
			}
		}
	}
}
