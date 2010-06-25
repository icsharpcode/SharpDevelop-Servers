using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using UsageDataAnalysisWebClient.Models;
using System.Data.Objects;
using System.Diagnostics;
using System.Web.UI.DataVisualization.Charting;

namespace UsageDataAnalysisWebClient.Controllers
{
    public class StabilityController : Controller
    {
        //
        // GET: /Stability/

        public ActionResult Index()
        {
			const int numberOfVersionGroups = 20;

			udcEntities db = new udcEntities();
			DateTime startDate = DateTime.Today.AddMonths(-12);
			var q =
				from s in db.Sessions
				where s.ClientSessionId != 0 // ignore welcome sessions
				where s.AppVersionRevision > 1 // ignore sessions with missing version info
				where s.StartTime >= startDate // only show releases in use last year
				where !s.EnvironmentDatas.Any(d => d.EnvironmentDataName.EnvironmentDataName1 == "branch" 
					|| d.EnvironmentDataName.EnvironmentDataName1 == "debug")
				let firstExceptionTime = s.Exceptions.Min(e => (DateTime?)e.ThrownAt)
				group new {
					IsCrashed = firstExceptionTime != null,
					IsKilled = s.EndTime == null,
					s.StartTime,
					CrashTime = firstExceptionTime ?? s.EndTime ?? s.FeatureUses.Max(f => (DateTime?)f.UseTime)
				} by new { s.UserId, Date = EntityFunctions.TruncateTime(s.StartTime), s.AppVersionRevision };
			
			Debug.WriteLine(((System.Data.Objects.ObjectQuery)q).ToTraceString());
			var resultList = (
				from g in q.AsEnumerable() // don't do this on DB, EF generates too slow SQL
				group g by new { g.Key.AppVersionRevision } into g
				orderby g.Key.AppVersionRevision
				select new {
					StartRevision = g.Key.AppVersionRevision,
					EndRevision = g.Key.AppVersionRevision,
					UserDaysWithCrash = g.Count(g2 => g2.Any(s => s.IsCrashed)),
					UserDaysWithKilled = g.Count(g2 => g2.Any(s => s.IsKilled)),
					UserDays = g.Count(),
					SessionsWithCrashOrKilled = g.Sum(g2 => g2.Count(s => s.IsKilled || s.IsCrashed)),
					TotalSessionLength = g.Sum(g2 => g2.Sum(s => Math.Max(0, ((s.CrashTime - s.StartTime) ?? new TimeSpan()).TotalHours)))
				}
			).ToList();

			foreach (var item in resultList) {
				Debug.WriteLine(item.ToString());
			}
			
			// group together little used versions into larger groups
			// warning: quadratic algorithm
			while (resultList.Count > numberOfVersionGroups) {
				int smallestIndex = 0;
				for (int i = 0; i < resultList.Count; i++) {
					if (resultList[i].UserDays < resultList[smallestIndex].UserDays)
						smallestIndex = i;
				}
				bool combineWithPrev;
				if (smallestIndex == 0)
					combineWithPrev = false;
				else if (smallestIndex == resultList.Count - 1)
					combineWithPrev = true;
				else
					combineWithPrev = resultList[smallestIndex - 1].UserDays < resultList[smallestIndex + 1].UserDays;
				if (combineWithPrev)
					smallestIndex--;
				var smallest = resultList[smallestIndex];
				var next = resultList[smallestIndex + 1];
				resultList[smallestIndex] =
					new {
						StartRevision = smallest.StartRevision,
						EndRevision = next.EndRevision,
						UserDaysWithCrash = next.UserDaysWithCrash + smallest.UserDaysWithCrash,
						UserDaysWithKilled = next.UserDaysWithKilled + smallest.UserDaysWithKilled,
						UserDays = next.UserDays + smallest.UserDays,
						SessionsWithCrashOrKilled = next.SessionsWithCrashOrKilled + smallest.SessionsWithCrashOrKilled,
						TotalSessionLength = next.TotalSessionLength + smallest.TotalSessionLength
					};
				resultList.RemoveAt(smallestIndex + 1);
			}

			Chart chart = new Chart();
			Series crashes = chart.Series.Add("Exceptions");
			Series killed = chart.Series.Add("Sessions without clean exit");
			crashes.ChartType = SeriesChartType.Line;
			killed.ChartType = SeriesChartType.Line;

			Chart chart2 = new Chart();
			Series crashFrequency = chart2.Series.Add("Crash Frequency");
			crashFrequency.ChartType = SeriesChartType.Line;

			foreach (var row in resultList) {
				string versionRange = "r" + row.StartRevision + (row.StartRevision == row.EndRevision ? "" : "-r" + row.EndRevision);
				crashes.Points.AddXY(versionRange, 100 * (double)row.UserDaysWithCrash / row.UserDays);
				killed.Points.AddXY(versionRange, 100 * (double)row.UserDaysWithKilled / row.UserDays);
				crashFrequency.Points.AddXY(versionRange, row.SessionsWithCrashOrKilled / row.TotalSessionLength);
			}
			ViewData.Model = chart;
			ViewData["CrashFrequency"] = chart2;
			return View();
        }
    }
}
