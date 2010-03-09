using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace ICSharpCode.UsageDataCollector.ServiceLibrary.ServiceImplementations
{
    public class DisposableUploadStream : IDisposable
    {
        private bool disposed = false;
        private Stream uploadStream = null;

        public DisposableUploadStream(Stream streamToWrap)
        {
            uploadStream = streamToWrap;
        }

        public Stream Stream
        {
            get
            {
                return uploadStream;
            }
        }

        ~DisposableUploadStream()
        {
            Dispose(false);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (false == disposed)
            {
                if (disposing)
                {
                    // Clean up all managed resources
                    if (null != uploadStream)
                    {
                        uploadStream.Close();
                        uploadStream.Dispose();
                    }
                }

                // Clean up all native resources - in this case NONE

                disposed = true;
            }
        }
    }

}
