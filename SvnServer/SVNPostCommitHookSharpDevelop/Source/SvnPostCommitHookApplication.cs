using System;
using System.Reflection;

// Configure log4net using the .config file
[assembly: log4net.Config.XmlConfigurator(Watch=true)]

namespace SvnPostCommitHook
{
	public class SvnPostCommitHookApplication
	{
		private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

		[STAThread]
		static void Main(string[] args)
		{
			if (args.Length < 2)
			{
				Console.WriteLine("Usage: post-commit <path to repository> <revision>");
				return;
			}

			string repos = args[0];
			string rev = args[1];
			log.Info(string.Format("Starting pull for {0}, rev {1}", repos, rev));

			try 
			{
				CommitInformation ci = new CommitInformation(repos, rev);
				ci.Read();
				ci.PostToMailingList();
			} 
			catch(Exception e) 
			{
				log.Fatal("Exception in Main()", e);
			}

			log.Info(string.Format("Finishing pull for {0}, rev {1}", repos, rev));
		}

		public static string InfoMessage()
		{
			AssemblyName an = Assembly.GetExecutingAssembly().GetName();
			return String.Format(" -- SvnPostCommitHook [internal] {0}.{1}.{2}.{3} -- ",
				an.Version.Major, an.Version.Minor, an.Version.Build, an.Version.Revision);
		}
	}
}
