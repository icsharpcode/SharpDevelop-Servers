using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using ICSharpCode.UsageDataCollector.Contracts;
using ICSharpCode.UsageDataCollector.ServiceLibrary.ServiceImplementations;

namespace ICSharpCode.UsageDataCollector.ServiceLibrary
{
    //
    // Wrapper for instantiation of custom UDC service implementations (mostly testing stubs)
    //
    public class UsageDataCollectorService : IUDCUploadService
    {
        private IUDCUploadService _active;

        public UsageDataCollectorService()
        {
            _active = new StoreLocallyUploadService();
        }

        public void UploadUsageData(UDCUploadRequest request)
        {
            _active.UploadUsageData(request);
        }
    }
}
