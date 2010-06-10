using System;
using System.Collections.Generic;

namespace UsageDataAnalysisWebClient.Models {
	public class ExceptionGroupEditModel {
		public int ExceptionGroupId { get; set; }

		public string TypeFingerprintSha256Hash { get; set; }

		public string ExceptionType { get; set; }

		public string ExceptionLocation { get; set; }

		public string ExceptionFingerprint { get; set; }

		public string UserComment { get; set; }

		public int? UserFixedInRevision { get; set; }

		public IEnumerable<ExceptionModel> Exceptions { get; set; }
	}
}