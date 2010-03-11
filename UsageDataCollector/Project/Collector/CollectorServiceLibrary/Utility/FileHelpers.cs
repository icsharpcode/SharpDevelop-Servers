using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace ICSharpCode.UsageDataCollector.ServiceLibrary.Utility
{
    public class FileHelpers
    {
        public static bool StoreUploadedStream(string localFileFullPath, Stream usageData)
        {
            if (null == usageData) return false;

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
            catch (Exception ex)
            {
                // TODO: log the error
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
