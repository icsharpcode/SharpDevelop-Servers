using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ICSharpCode.UsageDataCollector.DataAccess.Analysis;

namespace ExcelReport
{
    class SessionWithVersionNumber
    {
        public Session Session;
        public Version AppVersion;

        public DateTime StartTime { get { return Session.StartTime; } }
        public int UserId { get { return Session.UserId; } }

        public SessionWithVersionNumber(Session session, Version appVersion)
        {
            this.Session = session;
            this.AppVersion = appVersion;
        }
    }
}
