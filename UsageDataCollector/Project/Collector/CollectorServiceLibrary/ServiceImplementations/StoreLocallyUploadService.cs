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
            string sentAppKey = request.ApplicationKey;
            string storedAppKey = ConfigurationManager.AppSettings[UploadServiceConstants.AppSettings_ApplicationKey];

            if (0 != String.Compare(sentAppKey, storedAppKey, true))
            {
                // Invalid application key was sent, do not store message
                return;
            }

            string filePath = ConfigurationManager.AppSettings[appSettingsDropDirectoryKey] +
                                        Guid.NewGuid().ToString() + fileExtension;

            using (DisposableUploadStream us = new DisposableUploadStream(request.UsageData))
            {
                if (!FileHelpers.StoreUploadedStream(filePath, us.Stream))
                {
                    throw new Exception("An error occured");
                }
            }
        }
    }
}
