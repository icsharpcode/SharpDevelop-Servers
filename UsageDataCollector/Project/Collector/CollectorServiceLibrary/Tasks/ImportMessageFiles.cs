using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Build.Utilities;
using Microsoft.Build.Framework;
using ICSharpCode.UsageDataCollector.DataAccess.Collector;
using ICSharpCode.UsageDataCollector.Contracts;
using ICSharpCode.UsageDataCollector.ServiceLibrary.Import;
using System.Diagnostics;

namespace ICSharpCode.UsageDataCollector.ServiceLibrary.Tasks
{
    public class ImportMessageFiles : Task
    {
        private List<ITaskItem> messagesFailedToImport = new List<ITaskItem>();
        private ITaskItem[] messagesToImport;
        private ITaskItem connectionString;
		bool ignoreSessionsTooFarFromFileDate = true;

        [Required]
        public ITaskItem ConnectionString
        {
            get { return connectionString; }
            set { connectionString = value; }
        }

        [Required]
        public ITaskItem[] MessagesToImport
        {
            get { return messagesToImport; }
            set { messagesToImport = value; }
        }

		public bool IgnoreSessionsTooFarFromFileDate
		{
			get { return ignoreSessionsTooFarFromFileDate; }
			set { ignoreSessionsTooFarFromFileDate = value; }
		}

        [Output]
        public ITaskItem[] FailedToImport
        {
            get { return messagesFailedToImport.ToArray(); }
        }

        public override bool Execute()
        {
            bool result = true;

            if ((null == messagesToImport) || (0 == messagesToImport.Length))
            {
                // messagesFailedToImport is already newed as an empty list
                return true;
            }

            if (null == connectionString)
            {
                Log.LogError("Connection String for ADO.NET EF cannot be empty");
                return false;
            }

            // Import logic built for MSBuild
            Stopwatch watch = Stopwatch.StartNew();
            using (var context = CollectorRepository.CreateContext(connectionString.ItemSpec))
            {
                CollectorRepository repo = new CollectorRepository();
                repo.Context = context;

                foreach (ITaskItem msgFilename in messagesToImport)
                {
                    UsageDataMessage message = null;

                    try
                    {
                        message = FileImporter.ReadMessage(msgFilename.ItemSpec);
                    }
                    catch (System.Exception ex)
                    {
                        Log.LogErrorFromException(ex);
                        messagesFailedToImport.Add(new TaskItem(msgFilename.ItemSpec));

                        continue;
                    }

					if (ignoreSessionsTooFarFromFileDate) {
						// Acceptable sessions are between 1.5 months old and 3 days into the future.
						// All other sessions indicate a horribly wrong system time on the user's machine and will be ignored.
						DateTime fileDate = System.IO.File.GetLastWriteTimeUtc(msgFilename.ItemSpec);
						message.Sessions.RemoveAll(s => s.StartTime < fileDate.AddDays(-45) || s.StartTime > fileDate.AddDays(3));
					}

                    try
                    {
                        StoreMessageInSqlServer processor = new StoreMessageInSqlServer(message, repo);
                        processor.ProcessMessage();
                    }
                    catch (System.Exception ex)
                    {
                        Log.LogErrorFromException(ex);
                        messagesFailedToImport.Add(new TaskItem(msgFilename.ItemSpec));
                    }
                }
            }
            watch.Stop();
            Log.LogMessage("Imported " + (messagesToImport.Length - messagesFailedToImport.Count) + " messages in " + watch.Elapsed);

            return result;
        }
    }
}
