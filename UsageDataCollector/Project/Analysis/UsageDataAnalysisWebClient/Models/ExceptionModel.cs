using System;
using System.Collections.Generic;

namespace UsageDataAnalysisWebClient.Models {
	public class ExceptionModel {
		public DateTime ThrownAt { get; set; }

		public IEnumerable<EnvironmentDataModel> Environment { get; set; }

		public IEnumerable<ExceptionModelFeatureUse> PreviousFeatureUses { get; set; }

		public string Stacktrace { get; set; }

		public int UserId { get; set; }
	}

	public class ExceptionModelFeatureUse
	{
		public DateTime UseTime { get; set; }

		public string FeatureName { get; set; }

		public string ActivationMethod { get; set; }
	}
}