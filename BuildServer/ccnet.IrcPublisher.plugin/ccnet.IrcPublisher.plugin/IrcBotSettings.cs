// <file>
//     <copyright see="prj:///doc/copyright.txt"/>
//     <license see="prj:///doc/license.txt"/>
//     <author name="Daniel Grunwald"/>
//     <version>$Revision$</version>
// </file>

using System;

namespace ccnet.IrcPublisher.plugin
{
	public class IrcBotSettings
	{
		public readonly string Server;
		public readonly int Port;
		public readonly string Nick;
		public readonly string RealName;
		
		public IrcBotSettings(string server, int port, string nick, string realName)
		{
			if (server == null)
				throw new ArgumentNullException("server");
			if (nick == null)
				throw new ArgumentNullException("nick");
			if (realName == null)
				throw new ArgumentNullException("realName");
			this.Server = server;
			this.Port = port;
			this.Nick = nick;
			this.RealName = realName;
		}
		
		public override int GetHashCode()
		{
			return Server.GetHashCode() ^ Port ^ Nick.GetHashCode();
		}
		
		public override bool Equals(object obj)
		{
			IrcBotSettings other = obj as IrcBotSettings;
			if (other == null)
				return false;
			return this.Server == other.Server && this.Port == other.Port && this.Nick == other.Nick;
		}
	}
}
