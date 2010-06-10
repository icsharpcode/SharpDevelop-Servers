using System;
using System.Collections.Generic;

namespace UsageDataAnalysisWebClient.Models {
	public class SessionModel {
		public int UserId { get; set; }

		public IEnumerable<EnvironmentDataModel> EnvironmentDatas { get; set; }
	}
}