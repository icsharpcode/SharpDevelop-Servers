using System.Collections.Generic;
using UsageDataAnalysisWebClient.Models;

namespace UsageDataAnalysisWebClient.Repositories {
	public interface IExceptionGroupRepository {
		List<ExceptionGroupIndexModel> GetExceptionGroups();
		ExceptionGroupEditModel GetExceptionGroupById(int id);
		void Save(int exceptionGroupId, string userComment, int? userFixedInRevision);
	}
}