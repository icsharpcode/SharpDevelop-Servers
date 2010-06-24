using System.Collections.Generic;
using UsageDataAnalysisWebClient.Models;

namespace UsageDataAnalysisWebClient.Repositories {
	public interface IExceptionGroupRepository {
		List<ExceptionGroupIndexModelEntry> GetExceptionGroups(int minimumRevision, int maximumRevision, string branch);
		ExceptionGroupEditModel GetExceptionGroupById(int id);
		void Save(int exceptionGroupId, string userComment, int? userFixedInRevision);
	}
}