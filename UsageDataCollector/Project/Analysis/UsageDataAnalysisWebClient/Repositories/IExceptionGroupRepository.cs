using System.Collections.Generic;
using UsageDataAnalysisWebClient.Models;

namespace UsageDataAnalysisWebClient.Repositories {
	public interface IExceptionGroupRepository {
		List<ExceptionGroupIndexModelEntry> GetExceptionGroups(string startCommit, string endCommit);
		ExceptionGroupEditModel GetExceptionGroupById(int id);
		void Save(int exceptionGroupId, string userComment, string userFixedInCommitHash);
	}
}