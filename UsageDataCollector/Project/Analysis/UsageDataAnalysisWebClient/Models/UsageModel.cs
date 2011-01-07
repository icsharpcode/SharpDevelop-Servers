using System;
using System.Collections.Generic;
using System.Web.UI.DataVisualization.Charting;

namespace UsageDataAnalysisWebClient.Models {
	public class UsageViewModel
	{
		public UsageViewModel()
		{
			var start = DateTime.Today.AddMonths(-6);
			this.StartDate = new DateTime(start.Year, start.Month, 1);
			this.EndDate = DateTime.Today.AddDays(-3); // data takes a while to be uploaded, so don't show incomplete days
		}

		public DateTime StartDate { get; set; }
		public DateTime EndDate { get; set; }

		public Chart DailyUsers { get; set; }
		public Chart WeeklyUsers { get; set; }
		public Chart MonthlyUsers { get; set; }
	}

	public class UsageDataPoint
	{
		public DateTime Date { get; set; }
		public string Version { get; set; }
		public int UserCount { get; set; }
	}
}