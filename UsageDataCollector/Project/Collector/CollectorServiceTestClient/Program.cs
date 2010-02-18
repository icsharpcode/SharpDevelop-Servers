using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

using ICSharpCode.UsageDataCollector.ServiceReference;
using System.ServiceModel;

namespace CollectorServiceTestClient
{
    class Program
    {
        public static string serverUrl = "http://usagedatacollector.sharpdevelop.net/upload/UploadUsageData.svc";
        public static string localTestUrl = "http://localhost:4711/UDCUpload/UploadUsageData.svc";

        static void Main(string[] args)
        {
            FileStream toTransfer = new FileStream("SharpDevelopUsageData.xml.gz", FileMode.Open, FileAccess.Read);

            EndpointAddress epa = new EndpointAddress(localTestUrl);
			BasicHttpBinding binding = new BasicHttpBinding();
			binding.Security.Mode = BasicHttpSecurityMode.None;
			binding.TransferMode = TransferMode.Buffered;   // TODO: Why does it work Streamed on 3.5, not on 4.0?
			binding.MessageEncoding = WSMessageEncoding.Mtom;
			
            UDCUploadServiceClient client = new UDCUploadServiceClient(binding, epa);
            client.UploadUsageData("sharpdevelop", toTransfer);
        }
    }
}
