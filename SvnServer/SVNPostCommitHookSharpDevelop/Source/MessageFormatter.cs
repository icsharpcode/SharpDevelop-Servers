using System;

namespace SvnPostCommitHook
{
	/// <summary>
	/// The interface for any MessageFormatter
	/// </summary>
	public abstract class MessageFormatter
	{
		protected MessageFormatter() {}

		public abstract void Format(Mailer mailer, CommitInformation commit);
	}
}
