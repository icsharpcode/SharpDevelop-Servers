using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Build.Utilities;
using Microsoft.Build.Framework;
using System.Diagnostics;
using ICSharpCode.UsageDataCollector.DataAccess.Collector;
using System.Text.RegularExpressions;
using LibGit2Sharp;

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
		/// Whether to look for 'git-svn' in messages; used to adjust sessions created by SharpDevelop versions prior to the git migration.
		/// </summary>
		public bool EnableGitSvnImport { get; set; }

		public override bool Execute()
		{
			using (var context = CollectorRepository.CreateContext(this.ConnectionString)) {
				db = new CollectorRepository();
				db.Context = context;

				using (repo = new Repository(System.IO.Path.Combine(this.Directory, ".git"))) {
					foreach (var tag in repo.Tags) {
						ObjectId commitID = tag.IsAnnotated ? tag.Annotation.TargetId : tag.Target.Id;
						ImportTag(tag.Name, (LibGit2Sharp.Commit)repo.Lookup(commitID, GitObjectType.Commit), true);
					}
					foreach (var branch in repo.Branches) {
						if (branch.IsRemote && branch.Name.StartsWith(this.Remote + "/", StringComparison.Ordinal)) {
							string branchName = branch.Name.Substring(this.Remote.Length + 1);
							if (branchName != "HEAD") {
								ImportTag(branchName, branch.Tip, false);
							}
						}
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

		private void ImportTag(string name, LibGit2Sharp.Commit commit, bool isRelease)
		{
			Debug.WriteLine("ImportTag({0}, {1}, {2})", name, commit.Sha, isRelease);
			int? commitId = ImportCommit(commit);
			if (commitId == null)
				return;
			TaggedCommit tag = db.GetTag(name, isRelease);
			if (tag != null) {
				if (tag.CommitId == commitId)
					return;
				Debug.WriteLine("Update " + (isRelease ? "tag" : "branch") + " " + name + " (commit " + commit.Sha + ")");
				tag.CommitId = commitId.Value;
			} else {
				Debug.WriteLine("Import " + (isRelease ? "tag" : "branch") + " " + name + " (commit " + commit.Sha + ")");
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
		private int? ImportCommit(LibGit2Sharp.Commit gitCommit)
		{
			var gitCommit = (LibGit2Sharp.Commit)repo.Lookup(commitRef, GitObjectType.Commit);

		    // Test whether this commit is already imported
		    var commit = db.GetCommitByHash(gitCommit.Sha);
			if (commit != null)
				return commit.Id;

		    // Test whether the commit is too old to be imported
			if (gitCommit.Committer.When < EarliestCommitDate)
				return null;
		    
			List<int> parentCommits = new List<int>();
			foreach (LibGit2Sharp.Commit parent in gitCommit.Parents) {
				int? parentCommit = ImportCommit(parent);
				if (parentCommit != null)
					parentCommits.Add(parentCommit.Value);
			}
			return FinishImportCommit(gitCommit, parentCommits);
		}*/

		class Frame
		{
			public readonly LibGit2Sharp.Commit gitCommit;
			public int state; // 0 = initial state; 1 = check loop condition; 2 = within loop
			public int loopIndex;
			public List<int> parentCommitIds;

			public Frame(LibGit2Sharp.Commit gitCommit)
			{
				this.gitCommit = gitCommit;
			}
		}

		int? ImportCommit(LibGit2Sharp.Commit startingCommit)
		{
			// Using an explicit stack, because we would run into StackOverflowException when writing this
			// as a recursive method. See comment above "class Frame" for the recursive implementation.

			int? result = null; // variable used to store the return value (passed from one stack frame to the calling frame)
			Stack<Frame> stack = new Stack<Frame>();
			stack.Push(new Frame(startingCommit));
			while (stack.Count > 0) {
				Frame f = stack.Peek();
				switch (f.state) {
					case 0:
						// Test whether this commit is already imported
						var commit = db.GetCommitByHash(f.gitCommit.Sha);
						if (commit != null) {
							result = commit.Id;
							stack.Pop();
							break;
						}
						// Test whether the commit is too old to be imported
						if (f.gitCommit.Committer.When < EarliestCommitDate) {
							result = null;
							stack.Pop();
							break;
						}
						f.parentCommitIds = new List<int>();
						f.loopIndex = 0;
						goto case 1; // proceed to first loop iteration
					case 1:
						// check the loop condition (whether we have imported all necessary commits)
						if (f.loopIndex < f.gitCommit.Parents.Count()) {
							// recursive invocation (to fetch the commit ID of the parent)
							f.state = 2;
							stack.Push(new Frame(f.gitCommit.Parents.ElementAt(f.loopIndex)));
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

		int? FinishImportCommit(LibGit2Sharp.Commit gitCommit, List<int> parentCommits)
		{
			string commitHash = gitCommit.Sha;
			var commit = new DataAccess.Collector.Commit();
			commit.Hash = commitHash;
			commit.CommitDate = gitCommit.Committer.When.UtcDateTime;
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
