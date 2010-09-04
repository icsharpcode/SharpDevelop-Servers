// <file>
//     <copyright see="prj:///doc/copyright.txt"/>
//     <license see="prj:///doc/license.txt"/>
//     <author name="Daniel Grunwald"/>
//     <version>$Revision$</version>
// </file>

using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

using Meebey.SmartIrc4net;

namespace ccnet.IrcPublisher.plugin
{
	public class IrcBot
	{
		static readonly Dictionary<IrcBotSettings, IrcBot> bots = new Dictionary<IrcBotSettings, IrcBot>();
		
		public static IrcBot GetBot(IrcBotSettings settings)
		{
			IrcBot bot;
			lock (bots) {
				if (!bots.TryGetValue(settings, out bot)) {
					bot = new IrcBot(settings);
					bots[settings] = bot;
				}
			}
			return bot;
		}
		
		readonly IrcBotSettings settings;
		readonly IrcClient client = new IrcClient();
		readonly List<string> channels = new List<string>();
		
		private IrcBot(IrcBotSettings settings)
		{
			this.settings = settings;
			client.SendDelay = 750;
			client.OnDisconnected += new EventHandler(client_OnDisconnected);
			client.OnQueryMessage += new IrcEventHandler(client_OnQueryMessage);
			client.OnChannelMessage += new IrcEventHandler(client_OnChannelMessage);
			client.Encoding = new UTF8Encoding(false);
			
			client.Connect(settings.Server, settings.Port);
			client.Login(settings.Nick, settings.RealName);
			
			Thread t = new Thread(new ThreadStart(ListenThread));
			t.IsBackground = true;
			t.Start();
		}
		
		void client_OnChannelMessage(object sender, IrcEventArgs e)
		{
		}
		
		void client_OnQueryMessage(object sender, IrcEventArgs e)
		{
			// process private message sent to the bot
			if (e.Data.Message == "quit") {
				client.RfcQuit();
			}
		}
		
		void client_OnDisconnected(object sender, EventArgs e)
		{
			lock (bots) {
				bots.Remove(settings);
			}
		}
		
		void ListenThread()
		{
			try {
				try {
					client.Listen();
				} finally {
					client.Disconnect();
				}
			} catch (ThreadAbortException) {
				throw;
			} catch (Exception ex) {
				Console.WriteLine(ex.ToString());
			} finally {
				client_OnDisconnected(null, null);
			}
		}
		
		public void SendMessage(string channel, string message)
		{
			lock (channels) {
				if (!channels.Contains(channel)) {
					channels.Add(channel);
					client.RfcJoin(channel);
				}
			}
			client.SendMessage(SendType.Message, channel, message);
		}
	}
}
