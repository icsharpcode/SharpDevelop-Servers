using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ICSharpCode.UsageDataCollector.DataAccess.Analysis;
using System.Data.Objects;

namespace ExcelReport
{
    class ReportRepository
    {
        UsageDataAnalysisEntities context;

        public ReportRepository()
        {
            this.context = new UsageDataAnalysisEntities();
        }

        public ReportRepository(UsageDataAnalysisEntities context)
        {
            if (context == null)
                throw new ArgumentNullException("context");
            this.context = context;
        }

        public DateTime? MinimumDate, MaximumDate;
        public Version MinimumVersion, MaximumVersion;

        List<SessionWithVersionNumber> cachedSessions;

        public IEnumerable<SessionWithVersionNumber> Sessions
        {
            get
            {
                if (cachedSessions == null)
                {
                    IQueryable<Session> sessions = context.Sessions;
                    if (MinimumDate != null)
                        sessions = sessions.Where(s => s.StartTime >= MinimumDate.Value);
                    if (MaximumDate != null)
                        sessions = sessions.Where(s => s.StartTime <= MaximumDate.Value);

                    var q = from session in sessions
                            let version = (from data in session.EnvironmentDatas where data.EnvironmentDataName.Name == "appVersion" select data.Value).FirstOrDefault()
                            where version != null
                            select new { session, version };

                    var sessionsWithVersion = q.AsEnumerable().Select(p => new SessionWithVersionNumber(p.session, new Version(p.version)));
                    if (MinimumVersion != null)
                        sessionsWithVersion = sessionsWithVersion.Where(s => s.AppVersion >= MinimumVersion);
                    if (MaximumVersion != null)
                        sessionsWithVersion = sessionsWithVersion.Where(s => s.AppVersion <= MaximumVersion);
                    cachedSessions = sessionsWithVersion.ToList();
                }
                return cachedSessions;
            }
        }

        public IEnumerable<EnvironmentDataName> EnvironmentDataNames
        {
            get
            {
                return context.EnvironmentDataNames;
            }
        }
    }
}
