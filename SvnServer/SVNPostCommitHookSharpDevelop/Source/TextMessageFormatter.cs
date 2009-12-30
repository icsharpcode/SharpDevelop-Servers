using System;
using System.IO;
using System.Collections.Specialized;

namespace SvnPostCommitHook
{
	/// <summary>
	/// Summary description for TextMessageFormatter.
	/// </summary>
	public class TextMessageFormatter : MessageFormatter
	{

		public override void Format(Mailer mailer, CommitInformation commit)
		{
			StringWriter writer = new StringWriter();

			writer.WriteLine("Author: {0}", commit.LookInfo.Author);
			writer.WriteLine("Date: {0:yyyy-MM-dd hh:mm:ss}", commit.LookInfo.Timestamp);
			writer.WriteLine("Revision: {0}", commit.Revision);
			writer.WriteLine("Message:");
			writer.WriteLine(commit.LookInfo.Message);
			writer.WriteLine();
			writer.WriteLine();

			AppendLogMessage(writer, commit.LookInfo.Added, "Added");
			AppendLogMessage(writer, commit.LookInfo.Modified, "Modified");
			AppendLogMessage(writer, commit.LookInfo.Deleted, "Deleted");

			writer.WriteLine();
			writer.WriteLine();
			writer.WriteLine(SvnPostCommitHookApplication.InfoMessage());

			mailer.TextBody = writer.ToString();
		}

		private void AppendLogMessage(StringWriter writer, StringCollection coll, string Heading)
		{
			int nCollItems = coll.Count;

			if (nCollItems > 0)
			{
				writer.WriteLine();
				writer.WriteLine(Heading + ":");
				for (int i=0; i < nCollItems; i++)
				{
					writer.WriteLine(coll[i]);
				}
			}
		}
		

	}
}
