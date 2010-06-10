using System;
using System.Collections.Generic;
using System.Linq;
using UsageDataAnalysisWebClient.Models;
using Exception = UsageDataAnalysisWebClient.Models.Exception;

namespace UsageDataAnalysisWebClient.Repositories {
	public class ExceptionGroupRepository : IExceptionGroupRepository {

		private udcEntities _db;

		public List<ExceptionGroupIndexModel> GetExceptionGroups() {
			const int minimumAffectedUsers = 2;
			const int minimumRevision = 5570; // TODO: make this parameter user-editable

			_db = new udcEntities();
			var q = from ex in _db.Exceptions
					// filter exceptions:
					// don't show follow-up errors 
					where ex.IsFirstInSession
					// ignore errors in ancient versions of the app
					where ex.Session.AppVersionRevision >= minimumRevision
					group ex by ex.ExceptionGroup into g
					// search "first seen" and "last seen" in all exceptions within the exception group
					// (not just those matching the filters above)
					let sessionsForVersionSearch = g.Key.Exceptions.Select(ex => ex.Session).Where(s => s.AppVersionRevision > 0)
					let firstSeenVersion = sessionsForVersionSearch.FirstOrDefault(s => s.AppVersionRevision == sessionsForVersionSearch.Min(s2 => s2.AppVersionRevision))
					let lastSeenVersion = sessionsForVersionSearch.FirstOrDefault(s => s.AppVersionRevision == sessionsForVersionSearch.Max(s2 => s2.AppVersionRevision))
					// fill the ViewModel instance with all the data
					select new ExceptionGroupIndexModel {
						ExceptionGroupId = g.Key.ExceptionGroupId,
						ExceptionType = g.Key.ExceptionType,
						ExceptionLocation = g.Key.ExceptionLocation,
						UserComment = g.Key.UserComment,
						UserFixedInRevision = g.Key.UserFixedInRevision,
						AffectedUsers = g.Select(ex => ex.Session.UserId).Distinct().Count(),
						Occurrences = g.Count(),
						FirstSeenMajor = firstSeenVersion.AppVersionMajor,
						FirstSeenMinor = firstSeenVersion.AppVersionMinor,
						FirstSeenBuild = firstSeenVersion.AppVersionBuild,
						FirstSeenRevision = firstSeenVersion.AppVersionRevision,
						LastSeenMajor = lastSeenVersion.AppVersionMajor,
						LastSeenMinor = lastSeenVersion.AppVersionMinor,
						LastSeenBuild = lastSeenVersion.AppVersionBuild,
						LastSeenRevision = lastSeenVersion.AppVersionRevision,
					} into viewModel
					where viewModel.AffectedUsers >= minimumAffectedUsers
					orderby viewModel.AffectedUsers descending, viewModel.Occurrences descending
					select viewModel;
			return q.ToList();
		}

		public ExceptionGroupEditModel GetExceptionGroupById(int id) {
			_db = new udcEntities();
			ExceptionGroup exceptionGroup = _db.ExceptionGroups.First(eg => eg.ExceptionGroupId == id);
			ExceptionGroupEditModel editModel = new ExceptionGroupEditModel();
			editModel.ExceptionFingerprint = exceptionGroup.ExceptionFingerprint;
			editModel.ExceptionGroupId = exceptionGroup.ExceptionGroupId;
			editModel.ExceptionLocation = exceptionGroup.ExceptionLocation;
			editModel.ExceptionType = exceptionGroup.ExceptionType;
			List<ExceptionModel> exceptions = new List<ExceptionModel>();
			foreach(Exception exception in exceptionGroup.Exceptions) {
				ExceptionModel exceptionModel = new ExceptionModel();
				exceptionModel.IsFirstInSession = exception.IsFirstInSession;
				exceptionModel.Stacktrace = exception.Stacktrace;
				exceptionModel.ThrownAt = exception.ThrownAt;
				SessionModel sessionModel = new SessionModel();
				List<EnvironmentDataModel> environmentData = new List<EnvironmentDataModel>();
				foreach(EnvironmentData data in exception.Session.EnvironmentDatas) {
					EnvironmentDataModel dataModel = new EnvironmentDataModel();
					dataModel.EnvironmentDataName = data.EnvironmentDataName.EnvironmentDataName1;
					dataModel.EnvironmentDataValue = data.EnvironmentDataValue.EnvironmentDataValue1;
					environmentData.Add(dataModel);
				}
				sessionModel.EnvironmentDatas = environmentData;
				exceptionModel.Session = sessionModel;
				exceptions.Add(exceptionModel);
				}
			editModel.Exceptions = exceptions;
			return editModel;
		}

		public void Save(int exceptionGroupId, string userComment, int? userFixedInRevision) {
			_db = new udcEntities();
			ExceptionGroup exceptionGroup = _db.ExceptionGroups.First(eg => eg.ExceptionGroupId == exceptionGroupId);
			exceptionGroup.UserComment = userComment;
			exceptionGroup.UserFixedInRevision = userFixedInRevision;
			_db.SaveChanges();
		}
	}
}