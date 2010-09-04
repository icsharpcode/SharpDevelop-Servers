using System;
using System.Xml;
using Exortech.NetReflector;
using ThoughtWorks.CruiseControl.Core;
using ThoughtWorks.CruiseControl.Remote;

namespace ccnet.IrcPublisher.plugin
{
	[ReflectorType("ircpublisher")]
	public class IrcPublisher : ITask
	{
		#region Public properties
		
		[ReflectorProperty("server", Required = true)]
		public string Server { get; set; }
		
		[ReflectorProperty("port", Required = true)]
		public int Port { get; set; }
		
		[ReflectorProperty("room", Required = true)]
		public string Room { get; set; }
		
		[ReflectorProperty("nick", Required = false)]
		public string Nick { get; set; }
		
		[ReflectorProperty("realname", Required = false)]
		public string RealName { get; set; }
		
		[ReflectorProperty("revisionfile", Required = false)]
		public string RevisionFile { get; set; }
		
		#endregion
		
		public void Run(IIntegrationResult result)
		{
			if (result.Status == IntegrationStatus.Unknown)
				return;
			
			string commitHash;
			string revision = ReadRevision(out commitHash) ?? "?";
			
			string message = CreateStatus(result, revision, commitHash ?? new string('?', 40));
			SendIrcMessage(message);
		}
		
		string ReadRevision(out string commitHash)
		{
			commitHash = null;
			if (string.IsNullOrEmpty(this.RevisionFile))
				return null;
			XmlDocument doc = new XmlDocument();
			doc.Load(this.RevisionFile);
			XmlElement hash = doc.DocumentElement["commitHash"];
			if (hash != null)
				commitHash = hash.InnerText;
			XmlElement version = doc.DocumentElement["version"];
			if (version == null)
				return null;
			return version.InnerText;
		}
		
		void SendIrcMessage(string message)
		{
			Console.WriteLine(message);
			try {
				IrcBotSettings settings = new IrcBotSettings(
					this.Server, this.Port,
					this.Nick ?? "ccnetbot",
					this.RealName ?? "CCnet IRC Publisher");
				IrcBot bot = IrcBot.GetBot(settings);
				string[] channels = this.Room.Split(new char[] { ',', ';'}, StringSplitOptions.RemoveEmptyEntries);
				foreach (string channel in channels) {
					bot.SendMessage(channel, message);
				}
			} catch (Exception ex) {
				Console.WriteLine(ex.ToString());
			}
		}
		
		private static string CreateStatus(IIntegrationResult result, string revision, string hash)
		{
			if (result.Status == IntegrationStatus.Success) {
				return String.Format("{0} Build {1} successful (commit {2}, took {3}).", result.ProjectName, revision, hash.Substring(0, 8),
				                     (int)result.TotalIntegrationTime.TotalMinutes + ":" + result.TotalIntegrationTime.Seconds);
			} else {
				return String.Format("{0} Build {1} failed (commit {2}). See {3}", result.ProjectName, revision, hash.Substring(0, 8), result.ProjectUrl);
			}
		}
	}
}
