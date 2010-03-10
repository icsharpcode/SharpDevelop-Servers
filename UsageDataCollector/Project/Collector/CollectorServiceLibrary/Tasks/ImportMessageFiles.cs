using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Build.Utilities;
using Microsoft.Build.Framework;

namespace ICSharpCode.UsageDataCollector.ServiceLibrary.Tasks
{
    public class ImportMessageFiles : Task
    {
        private List<ITaskItem> messagesFailedToImport = new List<ITaskItem>();
        private ITaskItem[] messagesToImport;
        private ITaskItem connectionString;

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

            // TODO: Implement actual logic

            return result;
        }
    }
}
