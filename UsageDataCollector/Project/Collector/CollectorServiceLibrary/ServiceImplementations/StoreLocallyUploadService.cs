using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using ICSharpCode.UsageDataCollector.Contracts;
using ICSharpCode.UsageDataCollector.ServiceLibrary.Utility;
using System.Configuration;
using System.IO;

namespace ICSharpCode.UsageDataCollector.ServiceLibrary.ServiceImplementations
{
    public class StoreLocallyUploadService : IUDCUploadService
    {
        private static string appSettingsDropDirectoryKey = "StoreLocallyUploadService_DropDirectory";
        private static string fileExtension = ".xml.gz";
        private static string uploadingFileExtension = ".xml.gz.uploading";

        public void UploadUsageData(UDCUploadRequest request)
        {
            string sentAppKey = request.ApplicationKey;
            string storedAppKey = ConfigurationManager.AppSettings[UploadServiceConstants.AppSettings_ApplicationKey];

            if (0 != String.Compare(sentAppKey, storedAppKey, true))
            {
                // Invalid application key was sent, do not store message
                return;
            }

            string dropDirectory = ConfigurationManager.AppSettings[appSettingsDropDirectoryKey];
            string fileGuidPart = Guid.NewGuid().ToString();
            string uploadingFilePath = dropDirectory +  fileGuidPart + uploadingFileExtension;

            string finalFilePath = dropDirectory + fileGuidPart + fileExtension;

            using (DisposableUploadStream us = new DisposableUploadStream(request.UsageData))
            {
                if (FileHelpers.StoreUploadedStream(uploadingFilePath, us.Stream))
                {
                    // mark it as uploaded successfully by giving it the final file extension
                    File.Move(uploadingFileExtension, finalFilePath);
                }
                else
                {
                    throw new Exception("An error occured");
                }
            }
        }
    }
}
