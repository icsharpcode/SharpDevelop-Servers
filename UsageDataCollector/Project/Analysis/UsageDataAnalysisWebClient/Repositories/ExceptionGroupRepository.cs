using System;
using System.Collections.Generic;
using System.Linq;
using UsageDataAnalysisWebClient.Models;
using Exception = UsageDataAnalysisWebClient.Models.Exception;
using System.Diagnostics;

namespace UsageDataAnalysisWebClient.Repositories {
	public class ExceptionGroupRepository : IExceptionGroupRepository {

		private udcEntities _db = new udcEntities();

		public string GetLatestTagName()
		{
			DateTime minimumCommitAge = DateTime.Now.AddDays(-14);
			return (from tag in _db.TaggedCommits
					where tag.IsRelease
					join c in _db.Commits on tag.CommitId equals c.Id
					where c.CommitDate < minimumCommitAge
					orderby c.CommitDate descending
					select tag.Name
				   ).FirstOrDefault();
		}

		public List<ExceptionGroupIndexModelEntry> GetExceptionGroups(string startCommit, string endCommit)
		{
			SourceControlRepository scm = new SourceControlRepository(_db);

			int? startCommitId = FindCommitId(startCommit);
			int? endCommitId = FindCommitId(endCommit);
			var interestingCommitIds = scm.GetCommitsBetween(startCommitId, endCommitId).Select(c => c.Id).ToList();

			var q = from ex in _db.Exceptions
					where ex.IsFirstInSession
					where interestingCommitIds.Contains((int)ex.Session.CommitId)
					group ex by ex.ExceptionGroup into g
					// fill the ViewModel instance with all the data
					select new ExceptionGroupIndexModelEntry {
						ExceptionGroupId = g.Key.ExceptionGroupId,
						ExceptionType = g.Key.ExceptionType,
						ExceptionLocation = g.Key.ExceptionLocation,
						UserComment = g.Key.UserComment,
						UserFixedInCommitId = g.Key.UserFixedInCommitId,
						AffectedUsers = g.Select(ex => ex.Session.UserId).Distinct().Count(),
						Occurrences = g.Count()
					} into viewModel
					orderby viewModel.AffectedUsers descending, viewModel.Occurrences descending
					select viewModel;
			var exceptions = EvaluateQuery(q.Take(50));
			// Get "fixed in" hashes from in-memory commit list:
			foreach (var exception in exceptions) {
				if (exception.UserFixedInCommitId != null) {
					var fixedIn = scm.GetCommitById(exception.UserFixedInCommitId.Value);
					exception.UserFixedInCommitHash = fixedIn.Hash;

					// check whether the exception has re-occurred in any commit after the fix
					var allFixedInCommitIds = fixedIn.GetDescendants().Select(c => c.Id).ToList();
					exception.HasRepeatedAfterFixVersion = _db.Exceptions.Any(e => e.ExceptionGroupId == exception.ExceptionGroupId && allFixedInCommitIds.Contains((int)e.Session.CommitId));
				}
			}

			/*
			var displayedGroupIds = exceptions.Select(e => e.ExceptionGroupId).ToList();
			var q2 = (from ex in _db.Exceptions
					  where displayedGroupIds.Contains(ex.ExceptionGroupId)
					  group ex.Session.CommitId by ex.ExceptionGroupId).ToDictionary(g => g.Key);
			*/

			return exceptions;
		}

		private List<T> EvaluateQuery<T>(IQueryable<T> query)
		{
			Debug.WriteLine(((System.Data.Objects.ObjectQuery)query).ToTraceString());
			return query.ToList();
		}

		public ExceptionGroupEditModel GetExceptionGroupById(int id)
		{
			ExceptionGroup exceptionGroup = _db.ExceptionGroups.First(eg => eg.ExceptionGroupId == id);
			ExceptionGroupEditModel editModel = new ExceptionGroupEditModel();
			editModel.ExceptionFingerprint = exceptionGroup.ExceptionFingerprint;
			editModel.ExceptionGroupId = exceptionGroup.ExceptionGroupId;
			editModel.ExceptionLocation = exceptionGroup.ExceptionLocation;
			editModel.ExceptionType = exceptionGroup.ExceptionType;
			editModel.UserComment = exceptionGroup.UserComment;
			if (exceptionGroup.UserFixedInCommit != null)
				editModel.UserFixedInCommit = exceptionGroup.UserFixedInCommit.Hash;
			List<ExceptionModel> exceptions = new List<ExceptionModel>();
			foreach (Exception exception in exceptionGroup.Exceptions) {
				ExceptionModel exceptionModel = new ExceptionModel();
				exceptionModel.IsFirstInSession = exception.IsFirstInSession;
				exceptionModel.Stacktrace = exception.Stacktrace;
				exceptionModel.ThrownAt = exception.ThrownAt;
				SessionModel sessionModel = new SessionModel();
				List<EnvironmentDataModel> environmentData = new List<EnvironmentDataModel>();
				foreach (EnvironmentData data in exception.Session.EnvironmentDatas) {
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

		public void Save(int exceptionGroupId, string userComment, string userFixedInCommitHash) {
			ExceptionGroup exceptionGroup = _db.ExceptionGroups.First(eg => eg.ExceptionGroupId == exceptionGroupId);
			exceptionGroup.UserComment = userComment;
			exceptionGroup.UserFixedInCommitId = FindCommitId(userFixedInCommitHash);
			_db.SaveChanges();
		}

		public int? FindCommitId(string hashOrTagName)
		{
			if (string.IsNullOrEmpty(hashOrTagName))
				return null;
			Commit commit = _db.Commits.FirstOrDefault(c => c.Hash.StartsWith(hashOrTagName));
			if (commit != null) {
				return commit.Id;
			} else {
				var taggedCommit = _db.TaggedCommits.FirstOrDefault(c => c.Name == hashOrTagName);
				if (taggedCommit != null)
					return taggedCommit.CommitId;
			}
			return null;
		}	
	}
}