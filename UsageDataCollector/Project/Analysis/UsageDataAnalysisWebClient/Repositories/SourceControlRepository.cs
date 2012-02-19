using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using UsageDataAnalysisWebClient.Models;

namespace UsageDataAnalysisWebClient.Repositories
{
	public class SourceControlRepository
	{
		public static string GetLatestTagName(int minimumAgeInDays)
		{
			using (udcEntities db = new udcEntities()) {
				DateTime minimumCommitAge = DateTime.Now.AddDays(-minimumAgeInDays);
				return (from tag in db.TaggedCommits
						where tag.IsRelease
						join c in db.Commits on tag.CommitId equals c.Id
						where c.CommitDate < minimumCommitAge
						orderby c.CommitDate descending
						select tag.Name
					   ).FirstOrDefault();
			}
		}

		public static int? FindCommitId(string hashOrTagName)
		{
			if (string.IsNullOrEmpty(hashOrTagName))
				return null;
			using (udcEntities db = new udcEntities()) {
				var taggedCommit = db.TaggedCommits.FirstOrDefault(c => c.Name == hashOrTagName);
				if (taggedCommit != null)
					return taggedCommit.CommitId;
				Commit commit = db.Commits.FirstOrDefault(c => c.Hash.StartsWith(hashOrTagName));
				if (commit != null)
					return commit.Id;
				return null;
			}
		}

		static readonly object lockObj = new object();
		static SourceControlRepository cached;

		public static SourceControlRepository GetCached()
		{
			lock (lockObj) {
				if (cached != null && Math.Abs((cached.date - DateTime.UtcNow).TotalMinutes) < 3)
					return cached;
				using (udcEntities db = new udcEntities()) {
					cached = new SourceControlRepository(db);
				}
				return cached;
			}
		}

		DateTime date;
		Dictionary<int, SourceControlCommit> commits;
		
		public SourceControlRepository(udcEntities db)
		{
			date = DateTime.UtcNow;
			commits = (from c in db.Commits
					   select new SourceControlCommit {
						   Id = c.Id,
						   Hash = c.Hash,
						   Date = c.CommitDate
					   }).ToDictionary(c => c.Id);

			foreach (var rel in db.CommitRelations) {
				SourceControlCommit parent = commits[rel.ParentCommit];
				SourceControlCommit child = commits[rel.ChildCommit];
				parent.Children.Add(child);
				child.Parents.Add(parent);
			}
		}

		public SourceControlCommit GetCommitById(int id)
		{
			return commits[id];
		}

		public IEnumerable<SourceControlCommit> GetCommitsBetween(int? startId, int? endId)
		{
			if (endId.HasValue) {
				var set = commits[endId.Value].GetAncestors();
				if (startId.HasValue)
					set.IntersectWith(commits[startId.Value].GetDescendants());
				return set;
			} else if (startId.HasValue) {
				return commits[startId.Value].GetDescendants();
			} else {
				return commits.Values;
			}
		}
	}

	public class SourceControlCommit
	{
		public int Id;
		public string Hash;
		public DateTime Date;

		public List<SourceControlCommit> Parents = new List<SourceControlCommit>();
		public List<SourceControlCommit> Children = new List<SourceControlCommit>();

		public HashSet<SourceControlCommit> GetAncestors()
		{
			HashSet<SourceControlCommit> set = new HashSet<SourceControlCommit>();
			Queue<SourceControlCommit> queue = new Queue<SourceControlCommit>();
			queue.Enqueue(this);
			while (queue.Count > 0) {
				SourceControlCommit c = queue.Dequeue();
				if (set.Add(c)) {
					foreach (var parent in c.Parents)
						queue.Enqueue(parent);
				}
			}
			return set;
		}

		public HashSet<SourceControlCommit> GetDescendants()
		{
			HashSet<SourceControlCommit> set = new HashSet<SourceControlCommit>();
			Queue<SourceControlCommit> queue = new Queue<SourceControlCommit>();
			queue.Enqueue(this);
			while (queue.Count > 0) {
				SourceControlCommit c = queue.Dequeue();
				if (set.Add(c)) {
					foreach (var child in c.Children)
						queue.Enqueue(child);
				}
			}
			return set;
		}
	}
}