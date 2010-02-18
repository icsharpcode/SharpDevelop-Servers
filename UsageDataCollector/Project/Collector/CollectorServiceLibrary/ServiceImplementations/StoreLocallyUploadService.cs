using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using ICSharpCode.UsageDataCollector.Contracts;
using ICSharpCode.UsageDataCollector.ServiceLibrary.Utility;
using System.Configuration;

namespace ICSharpCode.UsageDataCollector.ServiceLibrary.ServiceImplementations
{
    public class StoreLocallyUploadService : IUDCUploadService
    {
        private static string appSettingsDropDirectoryKey = "StoreLocallyUploadService_DropDirectory";
        private static string fileExtension = ".xml.gz";

        public void UploadUsageData(UDCUploadRequest request)
        {
            string fileName = request.ApplicationKey;
            // TODO: verify application key

            string filePath = ConfigurationManager.AppSettings[appSettingsDropDirectoryKey] +
                                        Guid.NewGuid().ToString() + fileExtension;

            if (!FileHelpers.StoreUploadedStream(filePath, request.UsageData))
            {
                throw new Exception("An error occured");
            }
        }
    }
}
