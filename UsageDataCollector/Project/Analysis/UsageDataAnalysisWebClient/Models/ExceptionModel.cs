using System;

namespace UsageDataAnalysisWebClient.Models {
	public class ExceptionModel {
		public bool IsFirstInSession { get; set; }

		public DateTime ThrownAt { get; set; }

		public SessionModel Session { get; set; }

		public string Stacktrace { get; set; }
	}
}