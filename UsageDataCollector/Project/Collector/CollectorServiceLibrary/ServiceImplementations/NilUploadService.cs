using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using ICSharpCode.UsageDataCollector.Contracts;

namespace ICSharpCode.UsageDataCollector.ServiceLibrary.ServiceImplementations
{
    public class NilUploadService : IUDCUploadService
    {
        // A "do nothing" service for testing purposes of sample client applications
        // or for providing a testing service URL that doesn't create load on the server
        public void UploadUsageData(UDCUploadRequest request)
        {
            
        }
    }
}
