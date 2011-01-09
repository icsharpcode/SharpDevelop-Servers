using System;
using System.ComponentModel;
using System.Collections.Generic;

namespace UsageDataAnalysisWebClient.Models {
	public class ExceptionGroupEditModel {
		public int ExceptionGroupId { get; set; }

		public string TypeFingerprintSha256Hash { get; set; }

		public string ExceptionType { get; set; }

		public string ExceptionLocation { get; set; }

		public string ExceptionFingerprint { get; set; }

		[DisplayName("Comment")]
		public string UserComment { get; set; }

		public int? UserFixedInCommitId { get; set; } // commit id

		[DisplayName("Fixed in Commit")]
		public string UserFixedInCommitHash { get; set; } // commit hash

		public string UserFixedInCommit { get; set; } // friendly name for the fixed in version

		public int FirstOccurrenceCommitId { get; set; }
		public string FirstOccurrenceCommit { get; set; }
		public string FirstOccurrenceCommitHash { get; set; }
		
		public int LastOccurrenceCommitId { get; set; }
		public string LastOccurrenceCommit { get; set; }
		public string LastOccurrenceCommitHash { get; set; }

		public IEnumerable<ExceptionModel> Exceptions { get; set; }

		public List<Tuple<string, double>> CrashProbabilities { get; set; }
	}
}