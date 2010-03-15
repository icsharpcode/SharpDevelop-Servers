using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using ICSharpCode.UsageDataCollector.Contracts;
using ICSharpCode.UsageDataCollector.ServiceLibrary.ServiceImplementations;

using log4net;
using log4net.Config;

namespace ICSharpCode.UsageDataCollector.ServiceLibrary
{
    //
    // Wrapper for instantiation of custom UDC service implementations (mostly testing stubs)
    //
    public class UsageDataCollectorService : IUDCUploadService
    {
        private IUDCUploadService _active = null;
        private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        static UsageDataCollectorService()
        {
            XmlConfigurator.Configure();
        }

        public UsageDataCollectorService()
        {
            _active = GetDefaultService();

            UDCServiceBase svcbase = (_active as UDCServiceBase);
            if (null != svcbase)
            {
                svcbase.Logger = log;
            }
        }

        public IUDCUploadService GetDefaultService()
        {
            return new StoreLocallyUploadService();
        }

        public void UploadUsageData(UDCUploadRequest request)
        {
            _active.UploadUsageData(request);
        }
    }
}
