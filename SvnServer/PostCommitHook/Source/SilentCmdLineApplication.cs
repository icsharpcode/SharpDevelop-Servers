using System;
using System.Text;
using System.IO;
using System.CodeDom.Compiler;
using System.Collections.Specialized;

namespace SvnPostCommitHook
{
	public class SilentCmdLineApplication
	{
		public static StringCollection Execute(string strCmd)
		{
			string output = "";
			string error  = "";

			TempFileCollection tf = new TempFileCollection();
			Executor.ExecWaitWithCapture(strCmd, tf, ref output, ref error);

			StreamReader sr = File.OpenText(output);
			StringCollection coll = new StringCollection();
			string strLine = null;

			sr.ReadLine(); // skip first line

			while (null != (strLine = sr.ReadLine()))
			{
				coll.Add(strLine);	
			}
			sr.Close();

			File.Delete(output);
			File.Delete(error);

			return coll;
		}

		public static string Execute(string strCmd, bool bDropFirstLineOfOutput)
		{
			string output = "";
			string error  = "";

			TempFileCollection tf = new TempFileCollection();
			Executor.ExecWaitWithCapture(strCmd, tf, ref output, ref error);

			StreamReader sr = File.OpenText(output);
			StringBuilder strBuilder = new StringBuilder();
			string strLine = null;
			bool bFirstLine = true;

			while (null != (strLine = sr.ReadLine()))
			{
				if ("" != strLine)
				{
					if (bFirstLine && bDropFirstLineOfOutput)
					{
						bFirstLine = false;
						continue;
					}
					strBuilder.Append(strLine);
					strBuilder.Append("\r\n");
				}
			}
			sr.Close();

			File.Delete(output);
			File.Delete(error);

			return strBuilder.ToString();
		}
	}
}
