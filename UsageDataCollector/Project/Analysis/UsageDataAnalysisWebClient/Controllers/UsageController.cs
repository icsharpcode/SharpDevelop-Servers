using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using UsageDataAnalysisWebClient.Models;
using System.Data.SqlClient;
using System.Data;
using System.Configuration;
using System.Web.UI.DataVisualization.Charting;

namespace UsageDataAnalysisWebClient.Controllers
{
	[Authorize]
    public class UsageController : Controller
    {
        public ActionResult Index(UsageViewModel model)
        {
			//udcEntities db = new udcEntities();
			model.DailyUsers = CreateChart(model.StartDate, model.EndDate, 0);
			model.WeeklyUsers = CreateChart(GetStartOfWeek(model.StartDate), GetStartOfWeek(model.EndDate), 1);
			model.MonthlyUsers = CreateChart(GetStartOfMonth(model.StartDate), GetStartOfMonth(model.EndDate), 2);

			ViewData.Model = model;
            return View();
        }
		
		static DateTime GetStartOfWeek(DateTime time)
		{
			return time.AddDays(-(int)time.DayOfWeek);
		}

		static DateTime GetStartOfMonth(DateTime time)
		{
			return time.AddDays(1 - (int)time.Day);
		}

		static Chart CreateChart(DateTime startDate, DateTime endDate, int mode)
		{
			System.Web.UI.DataVisualization.Charting.Chart Chart2 = new System.Web.UI.DataVisualization.Charting.Chart();  
			Chart2.Width = 800;  
			Chart2.Height = 300;  
			Chart2.RenderType = RenderType.ImageTag;  

			Chart2.Palette = ChartColorPalette.BrightPastel;  
			Chart2.ChartAreas.Add("Series 1");
			Chart2.Legends.Add("Legend 1");
			List<UsageDataPoint> diagramData = GetUsageData(startDate, endDate, mode);
			var versions = diagramData.Select(d => d.Version).Distinct().OrderBy(v => v, new UsageDataAnalysisWebClient.Repositories.VersionNameComparer()).ToList();
			var allDates = diagramData.Select(d => d.Date).Distinct().OrderBy(d => d).ToList();
			foreach (var version in versions) {
				var s = Chart2.Series.Add(version);
				s.ChartType = SeriesChartType.StackedArea;
				var dict = diagramData.Where(d => d.Version == version).ToDictionary(d => d.Date, d => d.UserCount);
				foreach (var date in allDates) {
					int count;
					dict.TryGetValue(date, out count);
					// add a half week so that the data points are in the middle of the week
					s.Points.AddXY(date + (mode == 2 ? TimeSpan.FromDays(15) : mode == 1 ? TimeSpan.FromDays(3.5) : TimeSpan.Zero), count);
				}
			}
			return Chart2;
		}

		private static List<UsageDataPoint> GetUsageData(DateTime startDate, DateTime endDate, int mode)
		{
			List<UsageDataPoint> usageData = new List<UsageDataPoint>();
			using (var c = new SqlConnection(ConfigurationManager.ConnectionStrings["udcADO"].ConnectionString)) {
				c.Open();
				using (var command = c.CreateCommand()) {
					command.CommandText = "UserCount";
					command.CommandType = CommandType.StoredProcedure;
					command.Parameters.Add("startDate", SqlDbType.DateTime2).Value = startDate;
					command.Parameters.Add("endDate", SqlDbType.DateTime2).Value = endDate;
					command.Parameters.Add("mode", SqlDbType.Int).Value = mode;
					using (var reader = command.ExecuteReader()) {
						while (reader.Read())
							usageData.Add(new UsageDataPoint {
								Date = reader.GetDateTime(0),
								Version = reader.IsDBNull(1) ? "Other" : reader.GetString(1),
								UserCount = reader.GetInt32(2)
							});
					}
				}
			}
			return usageData;
		}

    }
}
