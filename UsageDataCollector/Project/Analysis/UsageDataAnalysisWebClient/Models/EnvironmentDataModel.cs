using System;
using System.Collections.Generic;
using System.Web.UI.DataVisualization.Charting;

namespace UsageDataAnalysisWebClient.Models {
	public class EnvironmentViewModel
	{
		public EnvironmentViewModel()
		{
			this.StartDate = GetStartOfWeek(DateTime.Today.AddMonths(-12));
			this.EndDate = GetStartOfWeek(DateTime.Today.AddDays(-7)); // data takes a while to be uploaded, so don't show incomplete week
		}

		static DateTime GetStartOfWeek(DateTime time)
		{
			return time.AddDays(-(int)time.DayOfWeek);
		}

		public DateTime StartDate { get; set; }
		public DateTime EndDate { get; set; }

		public List<Chart> Charts = new List<Chart>();
	}
	
	public class EnvironmentDataModel {
		public string EnvironmentDataName { get; set; }

		public string EnvironmentDataValue { get; set; }
	}
}