using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using UsageDataAnalysisWebClient.Models;
using System.Data.SqlClient;
using System.Data;
using System.Configuration;

namespace UsageDataAnalysisWebClient.Controllers
{
	[Authorize]
    public class UsageController : Controller
    {
        public ActionResult Index(UsageViewModel model)
        {
			//udcEntities db = new udcEntities();
			List<UsageDataPoint> usageData = new List<UsageDataPoint>();
			using (var c = new SqlConnection(ConfigurationManager.ConnectionStrings["udcADO"].ConnectionString)) {
				c.Open();
				using (var command = c.CreateCommand()) {
					command.CommandText = "DailyUsers";
					command.CommandType = CommandType.StoredProcedure;
					command.Parameters.Add("startDate", SqlDbType.DateTime2).Value = model.StartDate;
					command.Parameters.Add("endDate", SqlDbType.DateTime2).Value = model.EndDate;
					using (var reader = command.ExecuteReader()) {
						while (reader.Read())
							usageData.Add(new UsageDataPoint { Date = reader.GetDateTime(0), UserCount = reader.GetInt32(1) });
					}
				}
			}
			model.DiagramData = usageData;

			ViewData.Model = model;
            return View();
        }

    }
}
