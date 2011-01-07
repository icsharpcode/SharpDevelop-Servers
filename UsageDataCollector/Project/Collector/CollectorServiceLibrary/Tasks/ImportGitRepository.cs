using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Build.Utilities;
using GitSharp.Core.Transport;
using Microsoft.Build.Framework;
using GitSharp.Core;
using System.Diagnostics;
using ICSharpCode.UsageDataCollector.DataAccess.Collector;
using System.Text.RegularExpressions;

namespace ICSharpCode.UsageDataCollector.ServiceLibrary.Tasks
{
	public class ImportGitRepository : Task
	{
		public ImportGitRepository()
		{
			this.Remote = "origin";
			this.EarliestCommitDate = new DateTime(2010, 03, 01);
		}

		CollectorRepository db;
		Repository repo;

		[Required]
		public string ConnectionString { get; set; }

		[Required]
		public string Directory { get; set; }

		public string Remote { get; set; }

		public DateTime EarliestCommitDate { get; set; }

		public bool Fetch { get; set; }

		public override bool Execute()
		{
			using (var context = CollectorRepository.CreateContext(this.ConnectionString)) {
				db = new CollectorRepository();
				db.Context = context;

				using (repo = Repository.Open(this.Directory)) {
					if (Fetch) {
						using (Transport t = Transport.open(repo, this.Remote)) {
							t.RemoveDeletedRefs = true;
							t.fetch(NullProgressMonitor.Instance, null);
						}
					}
					foreach (var tag in repo.getTags()) {
						ImportTag(tag.Key, tag.Value.PeeledObjectId ?? tag.Value.ObjectId, true);
					}
					foreach (var branch in repo.RefDatabase.getRefs("refs/remotes/" + this.Remote + "/")) {
						if (branch.Key == "HEAD") continue;
						ImportTag(branch.Key, branch.Value.PeeledObjectId ?? branch.Value.ObjectId, false);
					}
				}
				db.Context.SaveChanges();
				db = null;
			}
			return true;
		}

		private void ImportTag(string name, ObjectId commit, bool isRelease)
		{
			int? commitId = ImportCommit(commit);
			if (commitId == null)
				return;
			TaggedCommit tag = db.GetTag(name, isRelease);
			if (tag != null) {
				if (tag.CommitId == commitId)
					return;
				Debug.WriteLine("Update " + (isRelease ? "tag" : "branch") + " " + name + " (commit " + commit + ")");
				tag.CommitId = commitId.Value;
			} else {
				Debug.WriteLine("Import " + (isRelease ? "tag" : "branch") + " " + name + " (commit " + commit + ")");
				tag = new TaggedCommit();
				tag.CommitId = commitId.Value;
				tag.Name = name;
				tag.IsRelease = isRelease;
				db.Context.TaggedCommits.AddObject(tag);
			}
			db.Context.SaveChanges();
		}

		// import svn-revision numbers from trunk only (because we can't identify the branch reliably otherwise)
		static readonly Regex gitSvnRegex = new Regex(@"^\s*git-svn-id: [a-z0-9.:/]+/trunk@(\d+) [0-9a-f-]+$");

		private int? ImportCommit(ObjectId commitRef)
		{
			var gitCommit = repo.MapCommit(commitRef);
			DateTime commitDate = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc) + TimeSpan.FromMilliseconds(gitCommit.Committer.When);
			if (commitDate < EarliestCommitDate)
				return null;

			string commitHash = commitRef.Name;
			bool isSVN = false;
			foreach (string messageLine in gitCommit.Message.Split('\n', '\r')) {
				Match m = gitSvnRegex.Match(messageLine);
				if (m.Success) {
					// use SVN revision numbers for commits imported from SVN
					commitHash = "r" + m.Groups[1].Value;
					isSVN = true;
					break;
				}
			}
			var commit = db.GetCommitByHash(commitHash);
			if (commit != null)
				return commit.Id;

			List<int> parentCommits = new List<int>();
			foreach (ObjectId parent in gitCommit.ParentIds) {
				int? parentCommit = ImportCommit(parent);
				if (parentCommit != null)
					parentCommits.Add(parentCommit.Value);
			}

			commit = new DataAccess.Collector.Commit();
			commit.Hash = commitHash;
			commit.CommitDate = commitDate;
			db.Context.Commits.AddObject(commit);
			db.Context.SaveChanges();
			Debug.WriteLine("Imported commit " + commitHash + " as " + commit.Id);

			// Fix existing sessions referencing this commit:
			IQueryable<Session> sessionsThatNeedUpdate;
			if (isSVN) {
				int svnRevision = int.Parse(commitHash.Substring(1));
				sessionsThatNeedUpdate = db.Context.Sessions.Where(s => s.AppVersionRevision == svnRevision);
			} else {
				sessionsThatNeedUpdate = from s in db.Context.Sessions
										 join ed in db.Context.EnvironmentDatas on s.Id equals ed.SessionId
										 join edn in db.Context.EnvironmentDataNames on ed.EnvironmentDataNameId equals edn.Id
										 join edv in db.Context.EnvironmentDataValues on ed.EnvironmentDataValueId equals edv.Id
										 where edn.Name == "commit" && edv.Name == commitHash
										 select s;
			}
			int updatedSessionsCount = 0;
			foreach (var session in sessionsThatNeedUpdate) {
				session.CommitId = commit.Id;
				updatedSessionsCount++;
			}
			if (updatedSessionsCount > 0)
				Debug.WriteLine("Updated " + updatedSessionsCount + " sessions referencing this commit");

			// Now create the relation to the parent commits:
			foreach (int parentCommit in parentCommits) {
				CommitRelation r = new CommitRelation();
				r.ParentCommit = parentCommit;
				r.ChildCommit = commit.Id;
				db.Context.CommitRelations.AddObject(r);
			}
			db.Context.SaveChanges();

			return commit.Id;
		}
	}
}
