using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ICSharpCode.UsageDataCollector.DataAccess.Collector
{
    public class CollectorRepository
    {
        public UDCContext Context { get; set; }

        public static UDCContext CreateContext()
        {
            return new UDCContext();
        }

        public User FindUserByGuid(string guid)
        {
            return Context.Users.FirstOrDefault(u => u.AssociatedGuid == guid);
        }


    }
}
