using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Build.Utilities;
using Microsoft.Build.Framework;
using ICSharpCode.UsageDataCollector.Contracts;
using System.IO;
using System.IO.Compression;
using System.Xml;
using System.Runtime.Serialization;

namespace ICSharpCode.UsageDataCollector.ServiceLibrary.Tasks
{
    public class ScrubMessageFiles : Task
    {
        private ITaskItem[] messagesToScrub;

        [Required]
        public ITaskItem[] MessagesToScrub
        {
            get { return messagesToScrub; }
            set { messagesToScrub = value; }
        }

        public override bool Execute()
        {
            bool result = true;

            if ((null == messagesToScrub) || (0 == messagesToScrub.Length))
            {
                return true;
            }

            // Scrub files: fix schema, remove PII from files (applies to debug builds)
            // Necessary only for older SharpDevelop-only message files, not official builds
            foreach (ITaskItem msgFilename in messagesToScrub)
            {
                string filename = msgFilename.ItemSpec;
                UsageDataMessage message = ReadMessage(filename);

                if (null == message)
                {
                    Log.LogError("Error reading {}", filename);
                    continue;
                }

                // TODO: scrub the message

                SaveMessage(filename, message);
            }

            return result;
        }

        private UsageDataMessage ReadMessage(string filename)
        {
            string text;

            using (FileStream fs = new FileStream(filename, FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                using (GZipStream gzip = new GZipStream(fs, CompressionMode.Decompress))
                {
                    using (StreamReader r = new StreamReader(gzip))
                    {
                        text = r.ReadToEnd();
                    }
                }
            }

            try
            {
                text = text.Replace("http://schemas.datacontract.org/2004/07/ICSharpCode.UsageDataCollector.DataContract", 
                                    "http://schemas.datacontract.org/2004/07/ICSharpCode.UsageDataCollector.Contracts");

                using (XmlTextReader tr = new XmlTextReader(new StringReader(text)))
                {
                    return (UsageDataMessage)new DataContractSerializer(typeof(UsageDataMessage)).ReadObject(tr);
                }
            }
            catch (System.Exception)
            {
                return null;
            }
        }

        private void SaveMessage(string filename, UsageDataMessage message)
        {
            using (FileStream fs = File.Open(filename, FileMode.Create, FileAccess.Write, FileShare.None))
            {
                using (GZipStream zip = new GZipStream(fs, CompressionMode.Compress))
                {
                    DataContractSerializer serializer = new DataContractSerializer(typeof(UsageDataMessage));
                    serializer.WriteObject(zip, message);
                }
            }
        }
    }
}
