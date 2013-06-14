using System;
using System.Collections.Generic;
using System.Linq;
using UsageDataAnalysisWebClient.Models;
using Exception = UsageDataAnalysisWebClient.Models.Exception;
using System.Diagnostics;
using UsageDataAnalysisWebClient.Controllers;
using System.Data.SqlClient;
using System.Configuration;
using System.Data;

namespace UsageDataAnalysisWebClient.Repositories {
	public class ExceptionGroupRepository : IExceptionGroupRepository {

		private udcEntities _db = new udcEntities();

		public List<ExceptionGroupIndexModelEntry> GetExceptionGroups(string startCommit, string endCommit)
		{
			Stopwatch w = Stopwatch.StartNew();
			SourceControlRepository scm = SourceControlRepository.GetCached();

			// Step 1: figure out the interesting commit IDs
			int? startCommitId = SourceControlRepository.FindCommitId(startCommit);
			int? endCommitId = SourceControlRepository.FindCommitId(endCommit);
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
			// get details for the exception group
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
					FirstOccurrenceCommitId = commits.OrderBy(c => c.CommitDate).FirstOrDefault().Id,
					LastOccurrenceCommitId = commits.OrderByDescending(c => c.CommitDate).FirstOrDefault().Id
				}).Single();

			// get friendly names for the commits
			List<int> interestingCommitIds = new List<int>();
			interestingCommitIds.Add(editModel.FirstOccurrenceCommitId);
			interestingCommitIds.Add(editModel.LastOccurrenceCommitId);
			if (editModel.UserFixedInCommitId != null)
				interestingCommitIds.Add((int)editModel.UserFixedInCommitId);

			var scm = SourceControlRepository.GetCached();
			var map = CreateCommitIdToVersionMap(interestingCommitIds);

			editModel.FirstOccurrenceCommitHash = scm.GetCommitById(editModel.FirstOccurrenceCommitId).Hash;
			editModel.FirstOccurrenceCommit = map.GetValueOrDefault(editModel.FirstOccurrenceCommitId);
			editModel.LastOccurrenceCommitHash = scm.GetCommitById(editModel.LastOccurrenceCommitId).Hash;
			editModel.LastOccurrenceCommit = map.GetValueOrDefault(editModel.LastOccurrenceCommitId);

			if (editModel.UserFixedInCommitId != null) {
				editModel.UserFixedInCommitHash = scm.GetCommitById((int)editModel.UserFixedInCommitId).Hash;
				editModel.UserFixedInCommit = map.GetValueOrDefault((int)editModel.UserFixedInCommitId) ?? editModel.UserFixedInCommitHash.Truncate(8);
			}
			
			// get statistics about this exception
			editModel.CrashProbabilities = GetCrashStatisticsForExceptionGroup(id);

			// get details about the exception instances
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
			exceptionGroup.UserFixedInCommitId = SourceControlRepository.FindCommitId(userFixedInCommitHash);
			_db.SaveChanges();
		}

		public List<Tuple<string, double>> GetCrashStatisticsForExceptionGroup(int exceptionGroupId)
		{
			var result = new List<Tuple<string,double>>();
			using (var c = new SqlConnection(ConfigurationManager.ConnectionStrings["udcADO"].ConnectionString)) {
				c.Open();
				using (var command = c.CreateCommand()) {
					command.CommandText = "[InstabilityForException]";
					command.CommandType = CommandType.StoredProcedure;
					command.Parameters.Add("@exceptionGroup", SqlDbType.Int).Value = exceptionGroupId;
					using (var reader = command.ExecuteReader()) {
						while (reader.Read()) {
							string versionName = reader.GetString(0);
							int totalUserDays = reader.GetInt32(1);
							int crashedUserDays = reader.GetInt32(2);
							result.Add(Tuple.Create(versionName, 100.0 * crashedUserDays / totalUserDays));
						}
					}
				}
			}
			return result.OrderBy(g => g.Item1, new VersionNameComparer()).ToList();
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