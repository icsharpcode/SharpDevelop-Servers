using System;

namespace SvnPostCommitHook
{
	public class CommitInformationRaw
	{
		// input parameters
		private string Repository;
		private string Revision;

		// Info gathered from svnlook utility
		private string Author;
		private string Info;
		private string Changed;

		public CommitInformationRaw(string repos, string rev)
		{
			Repository = repos;
			Revision = rev;
		}

		public void Read()
		{
			string strRevAndRepos = Revision + " " + Repository;
			string strSvnLookPath = TypedConfiguration.Instance.SvnLookPath;
			strSvnLookPath = "\"" + strSvnLookPath + "\"";
			Info = SilentCmdLineApplication.Execute(strSvnLookPath + " info -r " + strRevAndRepos, true);
			Changed = SilentCmdLineApplication.Execute(strSvnLookPath + " changed -r " + strRevAndRepos, true);
			Author = SilentCmdLineApplication.Execute(strSvnLookPath + " author -r " + strRevAndRepos, true);
		}
		
		public void PostToMailingList()
		{
			Mailer mailer = TypedConfiguration.Instance.Mailer;
			mailer.To = TypedConfiguration.Instance.MailTo;
			mailer.From = TypedConfiguration.Instance.MailFrom;
			mailer.SmtpHost = TypedConfiguration.Instance.MailServer;
			mailer.Subject = String.Format(TypedConfiguration.Instance.MailSubject, Revision, Author);
			mailer.TextBody = Info + "\r\n\r\n" + Changed + "\r\n\r\n" + SvnPostCommitHookApplication.InfoMessage();
			mailer.Send();
		}
	}
}
