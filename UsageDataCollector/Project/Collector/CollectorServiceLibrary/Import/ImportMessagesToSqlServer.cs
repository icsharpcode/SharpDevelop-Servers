using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using ICSharpCode.UsageDataCollector.Contracts;
using ICSharpCode.UsageDataCollector.DataAccess.Collector;
using System.IO;

namespace ICSharpCode.UsageDataCollector.ServiceLibrary.Import
{
    public class ImportMessagesToSqlServer
    {
        public static void ImportSingleMessage(string filename)
        {
            UsageDataMessage message = FileImporter.ReadMessage(filename);

            using (var context = CollectorRepository.CreateContext())
            {
                CollectorRepository repo = new CollectorRepository();
                repo.Context = context;

                StoreMessageInSqlServer processor = new StoreMessageInSqlServer(message, repo);
                processor.ProcessMessage();
            }
        }

        // list directory
        // etl each file separately
        // load file
        // match all four types & insert (if necessary); match in memory (performance)
        // match user & insert (if necessary); via sproc (Cache issue)
        // load session and values into database
        // delete file when done & no exceptions
        public static void ImportMessagesFromDirectory(string directory, bool KeepFile)
        {
            using (var context = CollectorRepository.CreateContext())
            {
                CollectorRepository repo = new CollectorRepository();
                repo.Context = context;

                foreach (string filename in Directory.EnumerateFiles(directory, "*.xml.gz", SearchOption.TopDirectoryOnly))
                {
                    UsageDataMessage message = null;
                    try
                    {
                        message = FileImporter.ReadMessage(filename);
                    }
                    catch (System.Exception ex)
                    {
                        Console.WriteLine("Failed to read file {0}, exception {1}", filename, ex.Message);
                        continue;
                    }

                    StoreMessageInSqlServer processor = new StoreMessageInSqlServer(message, repo);
                    processor.ProcessMessage();
                }
            }
        }
    }
}
