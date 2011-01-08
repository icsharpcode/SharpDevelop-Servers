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
			Stopwatch w = Stopwatch.StartNew();
			SourceControlRepository scm = SourceControlRepository.GetCached();

			// Step 1: figure out the interesting commit IDs
			int? startCommitId = FindCommitId(startCommit);
			int? endCommitId = FindCommitId(endCommit);
			var interestingCommitIds = new HashSet<int>(scm.GetCommitsBetween(startCommitId, endCommitId).Select(c => c.Id));

			// Step 2: retrieve all exception instances from the database
			IQueryable<Exception> exceptionInstances = _db.Exceptions;
			var exceptions = EvaluateQuery(
				from ex in exceptionInstances
				where ex.IsFirstInSession
				let s = ex.Session
				//where interestingCommitIds.Contains((int)s.CommitId) 
				// Entity framework is too slow with large lists; it's faster to just check for !=null and then filter out in memory
				where s.CommitId != null
				select new {
					SessionId = ex.SessionId,
					CommitId = (int)s.CommitId,
					UserId = s.UserId,
					ExceptionGroupId = ex.ExceptionGroupId
				});

			// Step 3: Figure out the most relevant groups from the exceptions (in memory)
			var exceptionGroups = (
				from ex in exceptions
				group ex by ex.ExceptionGroupId into g
				let interestingInstances = g.Where(ex => interestingCommitIds.Contains(ex.CommitId))
				select new {
					ExceptionGroupId = g.Key,
					AffectedUsers = interestingInstances.Select(ex => ex.UserId).Distinct().Count(),
					Occurrences = interestingInstances.Count(),
					CommitIds = g.Select(ex => ex.CommitId).Distinct() // use all instances, not just interesting ones
				} into g
				where g.Occurrences > 0
				orderby g.AffectedUsers descending, g.Occurrences descending
				select g
			).Take(50).ToList();

			// Step 4: Retrieve additional details from the database for our most relevant groups
			var exceptionGroupIds = exceptionGroups.Select(e => e.ExceptionGroupId).ToList();
			var exceptionGroupDetails = EvaluateQuery(
				from ex in _db.ExceptionGroups
				where exceptionGroupIds.Contains(ex.ExceptionGroupId)
				select new {
					ex.ExceptionGroupId,
					ex.ExceptionLocation,
					ex.ExceptionFingerprint,
					ex.ExceptionType,
					ex.UserFixedInCommitId,
					ex.UserComment
				});

			// Step 5: put together the viewmodel
			var viewModels = (
				from ex in exceptionGroups
				join details in exceptionGroupDetails on ex.ExceptionGroupId equals details.ExceptionGroupId
				let fixedIn = details.UserFixedInCommitId != null ? scm.GetCommitById((int)details.UserFixedInCommitId) : null
				let occurredIn = new HashSet<SourceControlCommit>(ex.CommitIds.Select(c => scm.GetCommitById(c)))
				let firstOccurredVersion = occurredIn.OrderBy(c => c.Date).First()
				let lastOccurredVersion = occurredIn.OrderByDescending(c => c.Date).First()
				select new ExceptionGroupIndexModelEntry {
					ExceptionGroupId = ex.ExceptionGroupId,
					ExceptionType = details.ExceptionType,
					ExceptionLocation = details.ExceptionLocation,
					UserComment = details.UserComment,
					UserFixedInCommitId = details.UserFixedInCommitId,
					UserFixedInCommitHash = fixedIn != null ? fixedIn.Hash : null,
					AffectedUsers = ex.AffectedUsers,
					Occurrences = ex.Occurrences,
					HasRepeatedAfterFixVersion = fixedIn != null && occurredIn.Overlaps(fixedIn.GetDescendants()),
					FirstSeenVersionCommitId = firstOccurredVersion.Id,
					FirstSeenVersionHash = firstOccurredVersion.Hash,
					LastSeenVersionCommitId = lastOccurredVersion.Id,
					LastSeenVersionHash = lastOccurredVersion.Hash
				}).ToList();

			// Step 6: Figure out friendly names for the versions involved
			var commitIds = (from v in viewModels select v.FirstSeenVersionCommitId)
				.Union(from v in viewModels select v.LastSeenVersionCommitId)
				.Union(from v in viewModels where v.UserFixedInCommitId != null select (int)v.UserFixedInCommitId)
				.ToList();
			var commitIdToVersionMap = CreateCommitIdToVersionMap(commitIds);

			// Step 7: Map friendly names onto view models:
			foreach (var v in viewModels) {
				v.LastSeenVersion = commitIdToVersionMap.GetValueOrDefault(v.LastSeenVersionCommitId);
				v.FirstSeenVersion = commitIdToVersionMap.GetValueOrDefault(v.FirstSeenVersionCommitId);
				if (v.UserFixedInCommitId != null)
					v.UserFixedInCommit = commitIdToVersionMap.GetValueOrDefault((int)v.UserFixedInCommitId) ?? v.UserFixedInCommitHash.Truncate(8);
			}

			Debug.WriteLine("All together: " + w.ElapsedMilliseconds + "ms");
			return viewModels;
		}

		Dictionary<int, string> CreateCommitIdToVersionMap(List<int> commitIds)
		{
			return EvaluateQuery((
				from s in _db.Sessions
				where commitIds.Contains((int)s.CommitId)
				select new {
					s.CommitId,
					s.AppVersionMajor,
					s.AppVersionMinor,
					s.AppVersionBuild,
					s.AppVersionRevision
				}).Distinct())
				.GroupBy(x => x.CommitId, x => x.AppVersionMajor + "." + x.AppVersionMinor + "." + x.AppVersionBuild + "." + x.AppVersionRevision)
				.ToDictionary(x => (int)x.Key, x => x.FirstOrDefault());
		}

		private List<T> EvaluateQuery<T>(IQueryable<T> query)
		{
			Debug.WriteLine(((System.Data.Objects.ObjectQuery)query).ToTraceString());
			Stopwatch w = Stopwatch.StartNew();
			var list = query.ToList();
			Debug.WriteLine("Query took " + w.ElapsedMilliseconds + "ms and returned " + list.Count + " rows");
			return list;
		}

		public ExceptionGroupEditModel GetExceptionGroupById(int id)
		{
			ExceptionGroupEditModel editModel = EvaluateQuery(
				from ex in _db.ExceptionGroups
				where ex.ExceptionGroupId == id
				let commits = (
					from e in ex.Exceptions
					let s = e.Session
					where s.CommitId != null
					select s.Commit
				)
				select new ExceptionGroupEditModel {
					ExceptionFingerprint = ex.ExceptionFingerprint,
					ExceptionGroupId = ex.ExceptionGroupId,
					ExceptionLocation = ex.ExceptionLocation,
					ExceptionType = ex.ExceptionType,
					UserComment = ex.UserComment,
					UserFixedInCommitId = ex.UserFixedInCommitId,
					FirstOccurranceCommitId = commits.OrderBy(c => c.CommitDate).FirstOrDefault().Id,
					LastOccurranceCommitId = commits.OrderByDescending(c => c.CommitDate).FirstOrDefault().Id
				}).Single();

			List<int> interestingCommitIds = new List<int>();
			interestingCommitIds.Add(editModel.FirstOccurranceCommitId);
			interestingCommitIds.Add(editModel.LastOccurranceCommitId);
			if (editModel.UserFixedInCommitId != null)
				interestingCommitIds.Add((int)editModel.UserFixedInCommitId);

			var scm = SourceControlRepository.GetCached();
			var map = CreateCommitIdToVersionMap(interestingCommitIds);

			editModel.FirstOccurranceCommitHash = scm.GetCommitById(editModel.FirstOccurranceCommitId).Hash;
			editModel.FirstOccurranceCommit = map.GetValueOrDefault(editModel.FirstOccurranceCommitId);
			editModel.LastOccurranceCommitHash = scm.GetCommitById(editModel.LastOccurranceCommitId).Hash;
			editModel.LastOccurranceCommit = map.GetValueOrDefault(editModel.LastOccurranceCommitId);

			if (editModel.UserFixedInCommitId != null) {
				editModel.UserFixedInCommitHash = scm.GetCommitById((int)editModel.UserFixedInCommitId).Hash;
				editModel.UserFixedInCommit = map.GetValueOrDefault((int)editModel.UserFixedInCommitId) ?? editModel.UserFixedInCommitHash.Truncate(8);
			}
			
			editModel.Exceptions = EvaluateQuery((
				from ex in _db.Exceptions
				where ex.ExceptionGroupId == editModel.ExceptionGroupId && ex.IsFirstInSession
				orderby ex.ThrownAt descending
				let session = ex.Session
				select new ExceptionModel {
					ThrownAt = ex.ThrownAt,
					Stacktrace = ex.Stacktrace,
					UserId = session.UserId,
					Environment =
						from ed in session.EnvironmentDatas
						select new EnvironmentDataModel {
							Name = ed.EnvironmentDataName.EnvironmentDataName1,
							Value = ed.EnvironmentDataValue.EnvironmentDataValue1
						},
					PreviousFeatureUses = (
						from fu in session.FeatureUses
						where fu.UseTime <= ex.ThrownAt
						orderby fu.UseTime descending
						select new ExceptionModelFeatureUse {
							UseTime = fu.UseTime,
							ActivationMethod = fu.ActivationMethod.ActivationMethodName,
							FeatureName = fu.Feature.FeatureName
						}
					).Take(5)
				}
				).Take(20)
			);
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

	static partial class ExtensionMethods
	{
		public static TValue GetValueOrDefault<TKey, TValue>(this Dictionary<TKey, TValue> dict, TKey key)
		{
			TValue val;
			if (dict.TryGetValue(key, out val))
				return val;
			else
				return default(TValue);
		}

		public static string Truncate(this string text, int length)
		{
			if (text == null || text.Length <= length)
				return text;
			else
				return text.Substring(0, length);
		}
	}
}