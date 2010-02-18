using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ICSharpCode.UsageDataCollector.Contracts;
using System.IO;
using System.IO.Compression;
using System.Runtime.Serialization;

namespace ICSharpCode.UsageDataCollector.ServiceLibrary.Import
{
    public class FileImporter
    {
        public static UsageDataMessage ReadMessage(string filename)
        {
            using (FileStream fs = new FileStream(filename, FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                using (GZipStream gzip = new GZipStream(fs, CompressionMode.Decompress))
                {
                    return (UsageDataMessage)new DataContractSerializer(typeof(UsageDataMessage)).ReadObject(gzip);
                }
            }
        }
    }
}
