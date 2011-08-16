using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ICSharpCode.UsageDataCollector.ServiceLibrary.Tasks;

namespace RepositoryImport
{
    class Program
    {
        static void Main(string[] args)
        {
            ImportGitRepository task = new ImportGitRepository();
            task.ConnectionString = "name=UDCContext";
            task.Directory = "c:\\sharpdevelop";
			task.EnableGitSvnImport = true;
            task.Execute();
        }
    }
}
