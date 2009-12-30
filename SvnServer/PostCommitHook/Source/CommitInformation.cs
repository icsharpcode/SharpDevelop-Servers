using System;
using System.IO;
using System.Text;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Web;
using System.Collections;

namespace SvnPostCommitHook
{
	public class CommitInformation
	{
		// Create a logger for use in this class
		private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

		// input parameters
		private string _repository;
		private string _revision;

		// Info gathered from svnlook utility
		private SvnLookOutputParser _lookInfo = null;

		public CommitInformation(string repos, string rev)
		{
			_repository = repos;
			_revision = rev;
		}

		public string Repository 
		{
			get { return _repository; }
		}

		public string Revision 
		{
			get { return _revision; }
		}

		public SvnLookOutputParser LookInfo 
		{
			get 
			{ 
				if (null == _lookInfo)
					log.Error("Accessing _lookinfo but it is null");

				return _lookInfo;
			}
		}

		public void Read()
		{
			// not really elegant, but sufficient as no recovery from an error condition is needed
			try
			{
				string strRevAndRepos = Revision + " " + Repository;
				string strSvnLookPath = TypedConfiguration.Instance.SvnLookPath;
				strSvnLookPath = "\"" + strSvnLookPath + "\"";

				if (log.IsInfoEnabled) log.Info(strSvnLookPath + " info -r " + strRevAndRepos);
				StringCollection infoColl = SilentCmdLineApplication.Execute(strSvnLookPath + " info -r " + strRevAndRepos);
				if (log.IsInfoEnabled) log.Info(strSvnLookPath + " changed -r " + strRevAndRepos);
				StringCollection changeColl = SilentCmdLineApplication.Execute(strSvnLookPath + " changed -r " + strRevAndRepos);

				StringCollection diffColl = null;
				if (TypedConfiguration.Instance.AppendDiffToMail)
				{
					if (log.IsInfoEnabled) log.Info(strSvnLookPath + " diff --no-diff-deleted -r " + strRevAndRepos);
					diffColl = SilentCmdLineApplication.Execute(strSvnLookPath + " diff --no-diff-deleted -r " + strRevAndRepos);
				}

				_lookInfo = SvnLookOutputParser.Parse(infoColl, changeColl, diffColl);
			}
			catch (Exception e)
			{
				log.Error("Read/Parse failed fatally", e);
			}
		}

		// SharpDevelop specific mail subject inferral
        private string InferSubjectFormatStringFromSvnPath(string svnPath)
        {
            string standardSubjectEnd = " rev {0}, {1}";

            StringDictionary mapSvnUrlToName = new StringDictionary();
            mapSvnUrlToName.Add("trunk/", "Mirador");
            mapSvnUrlToName.Add("branches/3.0/", "Montferrer");
            mapSvnUrlToName.Add("branches/2.1/", "Serralongue");
            mapSvnUrlToName.Add("branches/2.0/", "Corsavy");

            foreach (DictionaryEntry de in mapSvnUrlToName)
            {
                if (svnPath.StartsWith((string)de.Key))
                {
                    return "n:" + de.Value + standardSubjectEnd;
                }
            }

            if (svnPath.StartsWith("branches/"))
            {
                string[] pathParts = svnPath.Split(new char[] { '/' });
                if (pathParts.Length > 1)
                {
                    return "a:" + pathParts[1] + standardSubjectEnd;
                }
            }

            // Error case
            return "[Unidentified branch] rev {0}, {1}";
        }

        private string InferSubjectFormatString()
        {
            string mailingSubjectFormatString = "Foo rev {0}, {1}";

            if (LookInfo.Added.Count > 0)
            {
                mailingSubjectFormatString = InferSubjectFormatStringFromSvnPath(LookInfo.Added[0]);
            }
            else if (LookInfo.Modified.Count > 0)
            {
                mailingSubjectFormatString = InferSubjectFormatStringFromSvnPath(LookInfo.Modified[0]);
            }
            else if (LookInfo.Deleted.Count > 0)
            {
                mailingSubjectFormatString = InferSubjectFormatStringFromSvnPath(LookInfo.Deleted[0]);
            }

            return mailingSubjectFormatString;
        }

		public void PostToMailingList()
		{
			Mailer mailer = TypedConfiguration.Instance.Mailer;
			mailer.To = TypedConfiguration.Instance.MailTo;
			mailer.From = string.Format(TypedConfiguration.Instance.MailFrom, LookInfo.Author);
			mailer.SmtpHost = TypedConfiguration.Instance.MailServer;

			mailer.SmtpServerPort = TypedConfiguration.Instance.SmtpServerPort;
			mailer.SmtpAuthType = TypedConfiguration.Instance.SmtpAuthType;
			mailer.SmtpAuthUsername = TypedConfiguration.Instance.SmtpAuthUsername;
			mailer.SmtpAuthPassword = TypedConfiguration.Instance.SmtpAuthPassword;

            // hack for the SharpDevelop commit hook
            string mailingSubjectFormatString = InferSubjectFormatString();
            mailer.Subject = String.Format(mailingSubjectFormatString, Revision, LookInfo.Author);

			MessageFormatter formatter = new TextMessageFormatter();
			formatter.Format(mailer, this);

			if (!TypedConfiguration.Instance.MailTextOnly)
			{
				formatter = new HtmlMessageFormatter();
				formatter.Format(mailer, this);
			}

			try
			{
				mailer.Send();
			}
			catch (Exception e)
			{
				log.Error("Mailing commit message failed fatally", e);
			}
		}
			

		
	}
}
