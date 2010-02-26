using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Web.Caching;

using ICSharpCode.UsageDataCollector.DataAccess.Collector;
using System.Web;

namespace ICSharpCode.UsageDataCollector.ServiceLibrary.Import
{
    public sealed class ImportCache
    {
        public void ImplementCacheLater()
        {
            // HttpRuntime.Cache
        }

        // Singleton members
        ImportCache()
        {
        }

        public static ImportCache Instance
        {
            get
            {
                return Nested.instance;
            }
        }

        class Nested
        {
            static Nested()
            {
            }

            internal static readonly ImportCache instance = new ImportCache();
        }
    }

}
