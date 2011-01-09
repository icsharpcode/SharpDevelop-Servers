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
			this.EarliestCommitDate = new DateTime(2009, 09, 01);
		}

		CollectorRepository db;
		Repository repo;

		[Required]
		public string ConnectionString { get; set; }

		/// <summary>
		/// Directory that contains the git repository to read from.
		/// </summary>
		[Required]
		public string Directory { get; set; }

		/// <summary>
		/// Name of the git remote (default is "origin")
		/// </summary>
		public string Remote { get; set; }

		/// <summary>
		/// Minimum date for imported commits; all commits older than this limit are ignored.
		/// </summary>
		public DateTime EarliestCommitDate { get; set; }

		/// <summary>
		/// Whether to run "git fetch" in the repository.
		/// </summary>
		public bool Fetch { get; set; }

		/// <summary>
		/// Whether to look for 'git-svn' in messages; used to adjust sessions created by SharpDevelop versions prior to the git migration.
		/// </summary>
		public bool EnableGitSvnImport { get; set; }

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
				if (EnableGitSvnImport && svnRevisionToCommitIdMapping.Count > 0) {
					var map = svnRevisionToCommitIdMapping.OrderBy(p => p.Key).ToList();
					for (int i = 0; i < map.Count - 1; i++) {
						int minRev = map[i].Key;
						int maxRev = map[i + 1].Key - 1;
						foreach (var session in db.Context.Sessions.Where(s => s.AppVersionRevision >= minRev && s.AppVersionRevision <= maxRev)) {
							session.CommitId = map[i].Value;
						}
					}
					int lastRev = map.Last().Key;
					foreach (var session in db.Context.Sessions.Where(s => s.AppVersionRevision == lastRev)) {
						session.CommitId = map.Last().Value;
					}
					db.Context.SaveChanges();
				}
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

		Dictionary<int, int> svnRevisionToCommitIdMapping = new Dictionary<int, int>();

		/* Recursive implementation - runs into StackOverflowException
		private int? ImportCommit(ObjectId commitRef)
		{
			var gitCommit = repo.MapCommit(commitRef);

		    // Test whether this commit is already imported
		    var commit = db.GetCommitByHash(gitCommit.CommitId.Name);
			if (commit != null)
				return commit.Id;

		    // Test whether the commit is too old to be imported
			if (GetDate(gitCommit.Committer) < EarliestCommitDate)
				return null;
		    
			List<int> parentCommits = new List<int>();
			foreach (ObjectId parent in gitCommit.ParentIds) {
				int? parentCommit = ImportCommit(parent);
				if (parentCommit != null)
					parentCommits.Add(parentCommit.Value);
			}
			return FinishImportCommit(gitCommit, parentCommits);
		}*/

		class Frame
		{
			public readonly GitSharp.Core.Commit gitCommit;
			public int state; // 0 = initial state; 1 = check loop condition; 2 = within loop
			public int loopIndex;
			public List<int> parentCommitIds;

			public Frame(GitSharp.Core.Commit gitCommit)
			{
				this.gitCommit = gitCommit;
			}
		}

		int? ImportCommit(ObjectId commitRef)
		{
			// Using an explicit stack, because we would run into StackOverflowException when writing this
			// as a recursive method. See comment above "class Frame" for the recursive implementation.

			int? result = null; // variable used to store the return value (passed from one stack frame to the calling frame)
			Stack<Frame> stack = new Stack<Frame>();
			stack.Push(new Frame(repo.MapCommit(commitRef)));
			while (stack.Count > 0) {
				Frame f = stack.Peek();
				switch (f.state) {
					case 0:
						// Test whether this commit is already imported
						var commit = db.GetCommitByHash(f.gitCommit.CommitId.Name);
						if (commit != null) {
							result = commit.Id;
							stack.Pop();
							break;
						}
						// Test whether the commit is too old to be imported
						if (GetDate(f.gitCommit.Committer) < EarliestCommitDate) {
							result = null;
							stack.Pop();
							break;
						}
						f.parentCommitIds = new List<int>();
						f.loopIndex = 0;
						goto case 1; // proceed to first loop iteration
					case 1:
						// check the loop condition (whether we have imported all necessary commits)
						if (f.loopIndex < f.gitCommit.ParentIds.Length) {
							// recursive invocation (to fetch the commit ID of the parent)
							f.state = 2;
							stack.Push(new Frame(repo.MapCommit(f.gitCommit.ParentIds[f.loopIndex])));
							break;
						} else {
							// we are done with the loop; all parent commit IDs were collected
							result = FinishImportCommit(f.gitCommit, f.parentCommitIds);
							stack.Pop();
							break;
						}
					case 2:
						// store result from recursive invocation
						if (result != null)
							f.parentCommitIds.Add(result.Value);
						f.loopIndex++;
						goto case 1; // proceed to next loop iteration
				}
			}
			return result;
		}

		DateTime GetDate(GitSharp.Core.PersonIdent personIdent)
		{
			return new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc) + TimeSpan.FromMilliseconds(personIdent.When);
		}

		int? FinishImportCommit(GitSharp.Core.Commit gitCommit, List<int> parentCommits)
		{
			string commitHash = gitCommit.CommitId.Name;
			var commit = new DataAccess.Collector.Commit();
			commit.Hash = commitHash;
			commit.CommitDate = GetDate(gitCommit.Committer);
			db.Context.Commits.AddObject(commit);
			db.Context.SaveChanges();
			Debug.WriteLine("Imported commit " + commitHash + " as " + commit.Id);

			// Fix existing sessions referencing this commit:
			int? svnRevision = null;
			if (EnableGitSvnImport) {
				foreach (string messageLine in gitCommit.Message.Split('\n', '\r')) {
					Match m = gitSvnRegex.Match(messageLine);
					if (m.Success) {
						// determine SVN revision numbers for commits imported from SVN
						svnRevision = int.Parse(m.Groups[1].Value);
						break;
					}
				}
			}
			if (svnRevision != null) {
				svnRevisionToCommitIdMapping[svnRevision.Value] = commit.Id;
			} else {
				var sessionsThatNeedUpdate = from s in db.Context.Sessions
										 join ed in db.Context.EnvironmentDatas on s.Id equals ed.SessionId
										 join edn in db.Context.EnvironmentDataNames on ed.EnvironmentDataNameId equals edn.Id
										 join edv in db.Context.EnvironmentDataValues on ed.EnvironmentDataValueId equals edv.Id
										 where edn.Name == "commit" && edv.Name == commitHash
										 select s;
				int updatedSessionsCount = 0;
				foreach (var session in sessionsThatNeedUpdate) {
					session.CommitId = commit.Id;
					updatedSessionsCount++;
				}
				if (updatedSessionsCount > 0)
					Debug.WriteLine("Updated " + updatedSessionsCount + " sessions referencing this commit");
			}

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
