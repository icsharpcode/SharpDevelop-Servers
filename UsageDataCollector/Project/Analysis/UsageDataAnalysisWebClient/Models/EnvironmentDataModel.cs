using System;
using System.Collections.Generic;
using System.Web.UI.DataVisualization.Charting;
using UsageDataAnalysisWebClient.Controllers;

namespace UsageDataAnalysisWebClient.Models {
	public class EnvironmentViewModel
	{
		public EnvironmentViewModel()
		{
			this.StartDate = GetStartOfWeek(DateTime.Today.AddMonths(-12));
			this.EndDate = GetStartOfWeek(DateTime.Today.AddDays(-1));
		}

		static DateTime GetStartOfWeek(DateTime time)
		{
			return time.AddDays(-(int)time.DayOfWeek);
		}

		public DateTime StartDate { get; set; }
		public DateTime EndDate { get; set; }

		public List<EnvironmentViewChart> Charts;
	}

	public class EnvironmentViewChart
	{
		public string Title { get; set; }
		public int Id { get; set; }

		public int Width { get { return EnvironmentController.ChartWidth; } }
		public int Height { get { return EnvironmentController.ChartHeight; } }
	}
	
	public class EnvironmentDataModel {
		public string Name { get; set; }

		public string Value { get; set; }
	}
}