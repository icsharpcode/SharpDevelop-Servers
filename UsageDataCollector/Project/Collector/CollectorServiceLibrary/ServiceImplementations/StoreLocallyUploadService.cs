using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using ICSharpCode.UsageDataCollector.Contracts;
using System.Configuration;
using System.IO;
using log4net;

namespace ICSharpCode.UsageDataCollector.ServiceLibrary.ServiceImplementations
{
    public class StoreLocallyUploadService : UDCServiceBase, IUDCUploadService
    {
        private static string appSettingsDropDirectoryKey = "StoreLocallyUploadService_DropDirectory";
        private static string fileExtension = ".xml.gz";
        private static string uploadingFileExtension = ".xml.gz.uploading";

        public void UploadUsageData(UDCUploadRequest request)
        {
            string sentAppKey = request.ApplicationKey;
            string storedAppKey = ConfigurationManager.AppSettings[AppSettings_ApplicationKey];

            if (0 != String.Compare(sentAppKey, storedAppKey, true))
            {
                // Invalid application key was sent, do not store message
                if (log.IsErrorEnabled)
                    log.ErrorFormat("Received message with invalid application key {0}", sentAppKey);

                return;
            }

            string dropDirectory = ConfigurationManager.AppSettings[appSettingsDropDirectoryKey];
            string fileGuidPart = Guid.NewGuid().ToString();
            string uploadingFilePath = dropDirectory +  fileGuidPart + uploadingFileExtension;

            string finalFilePath = dropDirectory + fileGuidPart + fileExtension;

            using (DisposableUploadStream us = new DisposableUploadStream(request.UsageData))
            {
                if (StoreUploadedStream(uploadingFilePath, us.Stream))
                {
                    // mark it as uploaded successfully by giving it the final file extension
                    try
                    {
                        File.Move(uploadingFilePath, finalFilePath);
                    }
                    catch (System.Exception ex)
                    {
                        if (log.IsErrorEnabled)
                            log.Error("Final File.Move failed", ex);

                        throw new Exception("An error occured while processing the message upload");
                    }
                    
                    if (log.IsInfoEnabled)
                        log.InfoFormat("Message {0} stored successfully", fileGuidPart);
                }
                else
                {
                    // there is no point in telling the client details
                    throw new Exception("An error occured while processing the message upload"); 
                }
            }
        }
    }
}
