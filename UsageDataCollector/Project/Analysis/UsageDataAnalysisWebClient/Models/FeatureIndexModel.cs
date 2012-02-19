using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace UsageDataAnalysisWebClient.Models
{
	public class FeatureIndexModel
	{
		public string FeatureFilter { get; set; }
		public string VersionFilter { get; set; }

		public IEnumerable<FeatureIndexEntry> Entries { get; set; }

		public string ErrorMessage { get; set; }
	}

	public class FeatureIndexEntry
	{
		public int FeatureID { get; set; }
		public string FeatureName { get; set; }
		public int TotalUseCount { get; set; }
		public int UserDays { get; set; }
	}
}