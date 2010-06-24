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
			var names = db.EnvironmentDataNames.Select(n => new { Id = n.EnvironmentDataNameId, Name = n.EnvironmentDataName1 }).ToList();
			
			using (var c = new SqlConnection(ConfigurationManager.ConnectionStrings["udcADO"].ConnectionString)) {
				c.Open();
				foreach (var name in names) {
					var dicts = new List<DiagramSeries>();
					using (var command = c.CreateCommand()) {
						command.CommandText = "EnvironmentByWeek";
						command.CommandType = CommandType.StoredProcedure;
						command.Parameters.Add("startDate", SqlDbType.DateTime2).Value = model.StartDate;
						command.Parameters.Add("endDate", SqlDbType.DateTime2).Value = model.EndDate;
						command.Parameters.Add("envDataNameId", SqlDbType.Int).Value = name.Id;
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
					chart.Titles.Add(name.Name);
					List<DateTime> allDates = dicts.SelectMany(d => d.Keys).Distinct().OrderBy(d => d).ToList();
					if (dicts.Count > 10) {
						var dictsToCombine = dicts.OrderByDescending(d => d.Values.Sum()).Skip(9).ToList();
						dicts = dicts.OrderByDescending(d => d.Values.Sum()).Take(9).ToList();
						DiagramSeries other = new DiagramSeries { Name = "Other" };
						var q = from dict in dictsToCombine
							from pair in dict
							group pair.Value by pair.Key;
						foreach (var g in q) {
							other.Add(g.Key, g.Sum());
						}
						dicts.Add(other);
					}

					foreach (var dict in dicts) {
						Series series = chart.Series.Add(dict.Name);
						series.ChartType = SeriesChartType.StackedArea100;
						foreach (DateTime dt in allDates) {
							int count;
							dict.TryGetValue(dt, out count);
							series.Points.AddXY(dt, count);
						}
					}
					model.Charts.Add(chart);
				}
			}
			ViewData.Model = model;
            return View();
        }

    }
}
