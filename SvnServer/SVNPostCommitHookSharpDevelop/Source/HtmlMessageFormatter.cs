using System;
using System.IO;
using System.Text;
using System.Collections;
using System.Collections.Specialized;
using System.Web;

namespace SvnPostCommitHook
{
	/// <summary>
	/// Summary description for HtmlMessageFormatter.
	/// </summary>
	public class HtmlMessageFormatter : MessageFormatter
	{


		public override void Format(Mailer mailer, CommitInformation commit)
		{
			// {0} = revision
			// {1} = author
			// {2} = timestamp
			// {3} = message
			string htmlHeadAndMeta = @"
<!DOCTYPE HTML PUBLIC ""-//W3C//DTD HTML 4.01//EN"">
<html>
  <head>
    <title>Subversion Commit For Revision {0}</title>
  </head>
  <style>
{4}
  </style>
  <body>
    <h1>Commit log for Revision {0}</h1>
    <dl>
      <dt>Author</dt>
      <dd>{1}</dd>
      <dt>Timestamp</dt>
      <dd>{2:yyyy-MM-dd hh:mm:ss}</dd>
    </dl>
    <p id=""message"">{3}</p>";

			StringWriter writer = new StringWriter();
			string safeMessage = HttpUtility.HtmlEncode(commit.LookInfo.Message).Replace(Environment.NewLine, "<br />");
			writer.WriteLine(htmlHeadAndMeta, commit.Revision, commit.LookInfo.Author, commit.LookInfo.Timestamp, safeMessage, CssForHtml());
			AppendHtmlLogMessage(writer, commit.LookInfo.Added, "Added");
			AppendHtmlLogMessage(writer, commit.LookInfo.Modified, "Modified");
			AppendHtmlLogMessage(writer, commit.LookInfo.Deleted, "Deleted");

			AppendDiff(writer, commit.LookInfo.DiffLines);
			writer.WriteLine(@"<h5>{0}</h5>", SvnPostCommitHookApplication.InfoMessage());

			writer.WriteLine(@"</body></html>");

			mailer.HtmlBody = writer.ToString();
			
		}

		private void AppendHtmlLogMessage(StringWriter writer, StringCollection items, string heading) 
		{
			if(items.Count == 0) return;
			writer.WriteLine(@"<div id=""{0}"">", heading.ToLower());
			writer.WriteLine("<h2>{0}</h2>", heading);
			writer.WriteLine("<ul>");
			foreach(string s in items) 
			{
				writer.WriteLine(@"<li>{0}</li>", s);
			}
			writer.WriteLine("</ul>");
			writer.WriteLine("</div>");
		}

		private void AppendDiff(StringWriter writer, System.Collections.ICollection diffLines) 
		{
			// Reasons: * there is a switch in .config that disables gathering of diff data
			//          * there well could be no diff data at all for a given revision
			if (diffLines.Count == 0) return;

			string cssClass = "";
			bool inList = false;
			
			foreach(DiffLine line in diffLines) 
			{
				switch(line.Type) 
				{
					case DiffLineType.Addition:
						cssClass = "add";
						break;
					case DiffLineType.ChangeScope:
						cssClass = "scope";
						break;
					case DiffLineType.Comment:
						cssClass = "comment";
						break;
					case DiffLineType.Deletion:
						cssClass = "delete";
						break;
					case DiffLineType.Filename:
						if(inList) writer.WriteLine("</pre>");
						writer.Write("<pre class=\"diff\">");
						inList = true;
						cssClass = "filename";						
						break;
					case DiffLineType.NewRevisionFile:
						cssClass = "newrevision";
						break;
					case DiffLineType.OldRevisionFile:
						cssClass = "oldrevision";
						break;
					case DiffLineType.Separator:
						cssClass = "separator";
						break;
					case DiffLineType.Blank:
					case DiffLineType.Unchanged:
						cssClass = "unchanged";
						break;
					case DiffLineType.Unknown:
						cssClass = "unknown";
						break;

				}
				writer.WriteLine("<span class=\"{0}\">{1}</span>", cssClass, HttpUtility.HtmlEncode(line.Line));
			}

			writer.WriteLine("</pre>");
		}

		private void WriteItemWithClass(StringWriter writer, DiffLine line, string css) 
		{
			writer.WriteLine("<li class='{1}'>{0}</li>", line.Line, css);
		}

		private string CssForHtml() 
		{
			using(StreamReader reader = new StreamReader(TypedConfiguration.Instance.CssForHtmlMail)) 
			{
				return reader.ReadToEnd();
			}
		}
	}
}
