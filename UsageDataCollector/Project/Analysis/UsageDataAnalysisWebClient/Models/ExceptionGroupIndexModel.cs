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
        public int ExceptionGroupId { get; set; }
        public string ExceptionType { get; set; }
        public string ExceptionLocation { get; set; }
        public string UserComment { get; set; }
        public int? UserFixedInRevision { get; set; }
        public int AffectedUsers { get; set; }
        public int Occurrences { get; set; }

        public int? FirstSeenMajor { get; set; }
        public int? FirstSeenMinor { get; set; }
        public int? FirstSeenBuild { get; set; }
        public int? FirstSeenRevision { get; set; }

        public int? LastSeenMajor { get; set; }
        public int? LastSeenMinor { get; set; }
        public int? LastSeenBuild { get; set; }
        public int? LastSeenRevision { get; set; }

        public string FirstSeenVersion {
            get
            {
                return FirstSeenMajor + "." + FirstSeenMinor + "." + FirstSeenBuild + "." + FirstSeenRevision;
            }
        }
        public string LastSeenVersion {
            get
            {
                return LastSeenMajor + "." + LastSeenMinor + "." + LastSeenBuild + "." + LastSeenRevision;
            }
        }

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
                html = Regex.Replace(html, @"SD2-([0-9]+)", @"<a href=""http://bugtracker.sharpdevelop.net/Default.aspx?p=4&i=$1"">SD2-$1</a>");
                return new HtmlString(html);
            }
        }
    }
}