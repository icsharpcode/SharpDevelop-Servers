using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Text.RegularExpressions;

namespace UsageDataAnalysisWebClient.Models
{
	public class ExceptionGroupIndexModel
	{
		public ExceptionGroupIndexModel()
		{
			
		}

		public string StartCommitHash { get; set; }
		public string EndCommitHash { get; set; }

		public IEnumerable<ExceptionGroupIndexModelEntry> Entries { get; set; }
	}

    public class ExceptionGroupIndexModelEntry
    {
        public int ExceptionGroupId { get; set; }
        public string ExceptionType { get; set; }
        public string ExceptionLocation { get; set; }
        public string UserComment { get; set; }
		public int? UserFixedInCommitId { get; set; } // id
		public string UserFixedInCommit { get; set; } // friendly name (from a session's AppVersion)
        public string UserFixedInCommitHash { get; set; } // hash
		public bool HasRepeatedAfterFixVersion { get; set; }
        public int AffectedUsers { get; set; }
        public int Occurrences { get; set; }

		public int FirstSeenVersionCommitId { get; set; }
		public int LastSeenVersionCommitId { get; set; }
		public string FirstSeenVersionHash { get; set; }
		public string LastSeenVersionHash { get; set; }
		public string FirstSeenVersion { get; set; } // friendly name (from a session's AppVersion)
        public string LastSeenVersion { get; set; }

        public string ShortExceptionType
        {
            get
            {
                int pos = ExceptionType.LastIndexOf('.');
                if (pos >= 0)
                    return ExceptionType.Substring(pos + 1);
                else
                    return ExceptionType;
            }
        }

        public HtmlString RichUserComment {
            get
            {
                string html = HttpUtility.HtmlEncode(UserComment);
                if (html == null)
                    return null;
                html = Regex.Replace(html, @"SD-([0-9]+)", @"<a href=""http://bugtracker.sharpdevelop.net/Default.aspx?p=4&i=$1"">SD-$1</a>");
                return new HtmlString(html);
            }
        }
    }
}