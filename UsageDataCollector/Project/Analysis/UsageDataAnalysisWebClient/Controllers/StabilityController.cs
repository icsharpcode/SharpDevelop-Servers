using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using UsageDataAnalysisWebClient.Models;
using System.Data.Objects;
using System.Diagnostics;
using System.Web.UI.DataVisualization.Charting;
using UsageDataAnalysisWebClient.Repositories;

namespace UsageDataAnalysisWebClient.Controllers
{
    public class StabilityController : Controller
    {
        //
        // GET: /Stability/

        public ActionResult Index()
        {
			udcEntities db = new udcEntities();
			var q =
				from s in db.Sessions
				where s.ClientSessionId != 0 // ignore welcome sessions
				where !s.IsDebug // ignore debug builds
				join tag in db.TaggedCommits on s.CommitId equals tag.CommitId
				where tag.IsRelease // only use released versions, not branch 
				group new {
					IsCrashed = s.FirstException != null,
					IsKilled = s.EndTime == null,
					s.StartTime,
					CrashTime = s.FirstException ?? s.EndTime ?? s.LastFeatureUse
				} by new { s.UserId, Date = EntityFunctions.TruncateTime(s.StartTime), tag.Name };
			
			Debug.WriteLine(((System.Data.Objects.ObjectQuery)q).ToTraceString());
			var resultList = (
				from g in q.AsEnumerable() // don't do this on DB, EF generates too slow SQL
				group g by new { g.Key.Name } into g
				select new {
					TagName = g.Key.Name,
					UserDaysWithCrash = g.Count(g2 => g2.Any(s => s.IsCrashed)),
					UserDaysWithKilled = g.Count(g2 => g2.Any(s => s.IsKilled)),
					UserDays = g.Count(),
					SessionsWithCrashOrKilled = g.Sum(g2 => g2.Count(s => s.IsKilled || s.IsCrashed)),
					TotalSessionLength = g.Sum(g2 => g2.Sum(s => Math.Max(0, ((s.CrashTime - s.StartTime) ?? TimeSpan.Zero).TotalHours)))
				}
			).OrderBy(g => g.TagName, new VersionNameComparer()).ToList();

			foreach (var item in resultList) {
				Debug.WriteLine(item.ToString());
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
				crashes.Points.AddXY(row.TagName, 100 * (double)row.UserDaysWithCrash / row.UserDays);
				killed.Points.AddXY(row.TagName, 100 * (double)row.UserDaysWithKilled / row.UserDays);
				crashFrequency.Points.AddXY(row.TagName, row.SessionsWithCrashOrKilled / row.TotalSessionLength);
			}
			ViewData.Model = chart;
			ViewData["CrashFrequency"] = chart2;
			return View();
        }
    }
}
