using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ExcelReport.DataAccess;
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

        public IQueryable<Session> Sessions
        {
            get
            {
                IQueryable<Session> sessions = context.Sessions;
                if (MinimumDate != null)
                    sessions = sessions.Where(s => s.StartTime >= MinimumDate.Value);
                if (MaximumDate != null)
                    sessions = sessions.Where(s => s.StartTime <= MaximumDate.Value);

                if (MinimumVersion != null)
                    sessions = sessions.Where(s => s.AppVersionMajor > MinimumVersion.Major || s.AppVersionMajor == MinimumVersion.Major && (s.AppVersionMinor > MinimumVersion.Minor || s.AppVersionMinor == MinimumVersion.Minor && (s.AppVersionBuild > MinimumVersion.Build || s.AppVersionBuild == MinimumVersion.Build && s.AppVersionRevision >= MinimumVersion.Revision)));
                if (MaximumVersion != null)
                    sessions = sessions.Where(s => s.AppVersionMajor < MaximumVersion.Major || s.AppVersionMajor == MaximumVersion.Major && (s.AppVersionMinor < MaximumVersion.Minor || s.AppVersionMinor == MaximumVersion.Minor && (s.AppVersionBuild < MaximumVersion.Build || s.AppVersionBuild == MaximumVersion.Build && s.AppVersionRevision <= MaximumVersion.Revision)));
                return sessions;
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
