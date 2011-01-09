using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using UsageDataAnalysisWebClient.Models;
using System.Data;
using System.Configuration;
using System.Data.SqlClient;
using System.Web.UI.DataVisualization.Charting;
using UsageDataAnalysisWebClient.Repositories;
using System.IO;

namespace UsageDataAnalysisWebClient.Controllers
{
    public class EnvironmentController : Controller
    {
        //
        // GET: /Environment/

		class DiagramSeries : Dictionary<DateTime, int>
		{
			public string Name;
		}

        public ActionResult Index(EnvironmentViewModel model)
        {
			udcEntities db = new udcEntities();
			model.Charts = db.EnvironmentDataNames.Select(n => new EnvironmentViewChart { Id = n.EnvironmentDataNameId, Title = n.EnvironmentDataName1 }).ToList();
			model.Charts.RemoveAll(c => c.Title == "platform" || c.Title == "appVersion" || c.Title == "commit");
			ViewData.Model = model;
            return View();
        }

		public const int ChartWidth = 800;
		public const int ChartHeight = 300;

		public ActionResult Chart(DateTime startDate, DateTime endDate, string title, int id)
		{
			using (var c = new SqlConnection(ConfigurationManager.ConnectionStrings["udcADO"].ConnectionString)) {
				c.Open();
				var dicts = new List<DiagramSeries>();
				using (var command = c.CreateCommand()) {
					command.CommandText = "EnvironmentByWeek";
					command.CommandType = CommandType.StoredProcedure;
					command.Parameters.Add("startDate", SqlDbType.DateTime2).Value = startDate;
					command.Parameters.Add("endDate", SqlDbType.DateTime2).Value = endDate;
					command.Parameters.Add("envDataNameId", SqlDbType.Int).Value = id;
					using (var reader = command.ExecuteReader()) {
						DiagramSeries dict = null;
						while (reader.Read()) {
							string val = reader.IsDBNull(0) ? "unknown" : reader.GetString(0);
							if (dict == null || dict.Name != val) {
								dict = new DiagramSeries() { Name = val };
								dicts.Add(dict);
							}
							DateTime week = reader.GetDateTime(1);
							int count = reader.GetInt32(2);
							dict[week] = count;
						}
					}
				}
				Chart chart = new Chart();
				chart.Titles.Add(title);
				List<DateTime> allDates = dicts.SelectMany(d => d.Keys).Distinct().OrderBy(d => d).ToList();
				if (dicts.Count > 10) {
					var dictsToCombine = dicts.OrderByDescending(d => d.Values.Sum()).Skip(9).ToList();
					dicts = dicts.OrderByDescending(d => d.Values.Sum()).Take(9).OrderBy(d => d.Name, new VersionNameComparer()).ToList();
					DiagramSeries other = new DiagramSeries { Name = "Other" };
					var q = from dict in dictsToCombine
							from pair in dict
							group pair.Value by pair.Key;
					foreach (var g in q) {
						other.Add(g.Key, g.Sum());
					}
					dicts.Add(other);
				} else {
					dicts = dicts.OrderBy(d => d.Name, new VersionNameComparer()).ToList();
				}

				foreach (var dict in dicts) {
					Series series = chart.Series.Add(dict.Name);
					series.ChartType = SeriesChartType.StackedArea100;
					foreach (DateTime dt in allDates) {
						int count;
						dict.TryGetValue(dt, out count);
						series.Points.AddXY(dt + TimeSpan.FromDays(3.5), count); // add a half week so that the data points are in the middle of the week
					}
				}

				chart.Width = ChartWidth;
				chart.Height = ChartHeight;
				chart.ChartAreas.Add("Series 1");
				chart.Legends.Add("Legend 1");

				using (var ms = new MemoryStream()) {
					chart.SaveImage(ms, System.Web.UI.DataVisualization.Charting.ChartImageFormat.Png);
					ms.Position = 0;
					return File(ms.ToArray(), "image/png", title + ".png");
				}
			}
		}
    }
}
