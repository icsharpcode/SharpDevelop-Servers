using System;
using System.Collections.Generic;
using System.Linq;
using UsageDataAnalysisWebClient.Models;
using Exception = UsageDataAnalysisWebClient.Models.Exception;
using System.Diagnostics;

namespace UsageDataAnalysisWebClient.Repositories {
	public class ExceptionGroupRepository : IExceptionGroupRepository {

		private udcEntities _db = new udcEntities();

		public List<string> GetAllBranchNames()
		{
			var q = from data in _db.EnvironmentDatas
					where data.EnvironmentDataName.EnvironmentDataName1 == "branch"
					select data.EnvironmentDataValue.EnvironmentDataValue1;
			return new string[] { "all", "none" }.Concat(EvaluateQuery(q.Distinct().OrderBy(b => b))).ToList();
		}

		public List<ExceptionGroupIndexModelEntry> GetExceptionGroups(int minimumRevision, int maximumRevision, string branch)
		{
			var exceptions = _db.Exceptions.Where(ex => ex.IsFirstInSession); // filter exceptions: don't show follow-up errors 

			// ignore errors in ancient versions of the app
			if (minimumRevision > 0)
				exceptions = exceptions.Where(ex => ex.Session.AppVersionRevision >= minimumRevision);
			if (maximumRevision > 0)
				exceptions = exceptions.Where(ex => ex.Session.AppVersionRevision <= maximumRevision);
			if (branch != "all") {
				if (branch == "none") {
					exceptions = exceptions.Where(ex => !ex.Session.EnvironmentDatas.Any(data => data.EnvironmentDataName.EnvironmentDataName1 == "branch"));
				} else {
					exceptions = exceptions.Where(ex => ex.Session.EnvironmentDatas.Any(data => data.EnvironmentDataName.EnvironmentDataName1 == "branch"
						&& data.EnvironmentDataValue.EnvironmentDataValue1 == branch));
				}
			}

			var q = from ex in exceptions
					group ex by ex.ExceptionGroup into g
					// search "first seen" and "last seen" in all exceptions within the exception group
					// (not just those matching the filters above)
					let sessionsForVersionSearch = g.Key.Exceptions.Select(ex => ex.Session).Where(s => s.AppVersionRevision > 0)
					let firstSeenVersion = sessionsForVersionSearch.FirstOrDefault(s => s.AppVersionRevision == sessionsForVersionSearch.Min(s2 => s2.AppVersionRevision))
					let lastSeenVersion = sessionsForVersionSearch.FirstOrDefault(s => s.AppVersionRevision == sessionsForVersionSearch.Max(s2 => s2.AppVersionRevision))
					// fill the ViewModel instance with all the data
					select new ExceptionGroupIndexModelEntry {
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
					orderby viewModel.AffectedUsers descending, viewModel.Occurrences descending
					select viewModel;
			return EvaluateQuery(q.Take(50));
		}

		private List<T> EvaluateQuery<T>(IQueryable<T> query)
		{
			Debug.WriteLine(((System.Data.Objects.ObjectQuery)query).ToTraceString());
			return query.ToList();
		}

		public ExceptionGroupEditModel GetExceptionGroupById(int id) {
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
			ExceptionGroup exceptionGroup = _db.ExceptionGroups.First(eg => eg.ExceptionGroupId == exceptionGroupId);
			exceptionGroup.UserComment = userComment;
			exceptionGroup.UserFixedInRevision = userFixedInRevision;
			_db.SaveChanges();
		}
	}
}