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

        public IEnumerable<string> GetEnvironmentDataNameNames()
        {
            return Context.EnvironmentDataNames.Select(dn => dn.Name);
        }

        public IEnumerable<EnvironmentDataName> GetEnvironmentDataNames()
        {
            return Context.EnvironmentDataNames.Select(dn => dn);
        }

        public IEnumerable<string> GetActivationMethodNames()
        {
            return Context.ActivationMethods.Select(am => am.Name);
        }

        public IEnumerable<ActivationMethod> GetActivationMethods()
        {
            return Context.ActivationMethods.Select(am => am);
        }

        public IEnumerable<string> GetFeatureNames()
        {
            return Context.Features.Select(f => f.Name);
        }

        public IEnumerable<Feature> GetFeatures()
        {
            return Context.Features.Select(f => f);
        }
    }
}
