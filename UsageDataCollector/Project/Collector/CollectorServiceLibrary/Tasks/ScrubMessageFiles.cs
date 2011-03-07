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
using System.Text.RegularExpressions;

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

        // finds full paths ("in " drive letter ":\" ...)
        static readonly Regex stacktraceRegex = new Regex(@" in \w:\\.+$", RegexOptions.Multiline | RegexOptions.Compiled);

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
                try
                {
					bool isModified;
                    UsageDataMessage message = ReadMessage(filename, out isModified);

                    if (null == message)
                    {
                        Log.LogWarning("Error reading {0}", filename);
                        continue;
                    }

                    bool hasShownMessageForThisFile = false;
                    foreach (UsageDataException exception in message.Sessions.SelectMany(s => s.Exceptions))
                    {
                        if (stacktraceRegex.IsMatch(exception.StackTrace))
                        {
                            // Old SharpDevelop versions could upload stack trace information including full paths, which might
                            // contain private data (e.g. C:\Users\USERNAME). We'll remove full paths from stack traces.
                            // Newer UDC versions do not transmit paths anymore.
                            exception.StackTrace = stacktraceRegex.Replace(exception.StackTrace, "");
							isModified = true;
                            if (!hasShownMessageForThisFile)
                            {
                                Log.LogMessage("Removing potentially private information from " + filename);
                                hasShownMessageForThisFile = true;
                            }
                        }
                    }

					if (isModified)
						SaveMessage(filename, message);
                }
                catch (Exception ex)
                {
                    Log.LogErrorFromException(ex);
                    continue;
                }
            }

            return result;
        }

        private UsageDataMessage ReadMessage(string filename, out bool isModified)
        {
			isModified = false;
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
				if (text.Contains("http://schemas.datacontract.org/2004/07/ICSharpCode.UsageDataCollector.DataContract")) {
					text = text.Replace("http://schemas.datacontract.org/2004/07/ICSharpCode.UsageDataCollector.DataContract",
										"http://schemas.datacontract.org/2004/07/ICSharpCode.UsageDataCollector.Contracts");
					isModified = true;
				}

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
			DateTime fileDate = File.GetLastWriteTimeUtc(filename);
            using (FileStream fs = File.Open(filename, FileMode.Create, FileAccess.Write, FileShare.None))
            {
                using (GZipStream zip = new GZipStream(fs, CompressionMode.Compress))
                {
                    DataContractSerializer serializer = new DataContractSerializer(typeof(UsageDataMessage));
                    serializer.WriteObject(zip, message);
                }
            }
			File.SetLastAccessTimeUtc(filename, fileDate);
        }
    }
}
