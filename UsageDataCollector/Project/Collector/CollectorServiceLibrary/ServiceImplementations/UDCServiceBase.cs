using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using log4net;
using System.IO;

namespace ICSharpCode.UsageDataCollector.ServiceLibrary.ServiceImplementations
{
    public class UDCServiceBase
    {
        public static string AppSettings_ApplicationKey = "UploadService_ApplicationKey";

        protected ILog log = null;

        public ILog Logger
        {
            set
            {
                log = value;
            }
        }

        public bool StoreUploadedStream(string localFileFullPath, Stream usageData)
        {
            if (null == usageData)
            {
                if (log.IsErrorEnabled)
                    log.ErrorFormat("Usage Data stream was empty");

                return false;
            }

            bool bUploadSucceeded = true;
            FileStream fs = null;

            try
            {
                fs = File.Create(localFileFullPath);
                byte[] buffer = new byte[4096];
                int read = 0;
                while ((read = usageData.Read(buffer, 0, buffer.Length)) != 0)
                {
                    fs.Write(buffer, 0, read);
                }
            }
            catch (System.Exception ex)
            {
                if (log.IsErrorEnabled)
                    log.Error("Saving the uploaded stream failed", ex);

                bUploadSucceeded = false;
            }
            finally
            {
                if (null != fs)
                {
                    fs.Close();
                    fs.Dispose();

                    if (!bUploadSucceeded)
                    {
                        File.Delete(localFileFullPath);
                    }
                }

                // usageData stream is closed by using-idisposable pattern outside this method
            }
            return bUploadSucceeded;
        }
    }
}
