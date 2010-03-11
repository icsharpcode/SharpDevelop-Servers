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
        private static string cacheGetEnvironmentDataNameNames = "GetEnvironmentDataNameNames";
        private static string cacheGetEnvironmentDataNames = "GetEnvironmentDataNames";
        private static string cacheGetActivationMethodNames = "GetActivationMethodNames";
        private static string cacheGetActivationMethods = "GetActivationMethods";
        private static string cacheGetFeatureNames = "GetFeatureNames";
        private static string cacheGetFeatures = "GetFeatures";
        private static string cacheGetExceptionGroupFingerprintHashes = "GetExceptionGroupFingerprintHashes";
        private static string cacheGetExceptionGroups = "GetExceptionGroups";

        public static IEnumerable<string> GetEnvironmentDataNameNames(CollectorRepository repository)
        {
            IEnumerable<string> cached = HttpRuntime.Cache[cacheGetEnvironmentDataNameNames] as IEnumerable<string>;

            if (null == cached)
            {
                cached = repository.GetEnvironmentDataNameNames();
                HttpRuntime.Cache.Insert(cacheGetEnvironmentDataNameNames, cached);
            }

            return cached;
        }

        public static IEnumerable<EnvironmentDataName> GetEnvironmentDataNames(CollectorRepository repository)
        {
            IEnumerable<EnvironmentDataName> cached = HttpRuntime.Cache[cacheGetEnvironmentDataNames] as IEnumerable<EnvironmentDataName>;

            if (null == cached)
            {
                cached = repository.GetEnvironmentDataNames();
                HttpRuntime.Cache.Insert(cacheGetEnvironmentDataNames, cached);
            }

            return cached;
        }

        public static void InvalidateEnvironmentDataNamesCaches()
        {
            HttpRuntime.Cache.Remove(cacheGetEnvironmentDataNameNames);
            HttpRuntime.Cache.Remove(cacheGetEnvironmentDataNames);
        }

        public static IEnumerable<string> GetActivationMethodNames(CollectorRepository repository)
        {
            IEnumerable<string> cached = HttpRuntime.Cache[cacheGetActivationMethodNames] as IEnumerable<string>;

            if (null == cached)
            {
                cached = repository.GetActivationMethodNames();
                HttpRuntime.Cache.Insert(cacheGetActivationMethodNames, cached);
            }

            return cached;
        }

        public static IEnumerable<ActivationMethod> GetActivationMethods(CollectorRepository repository)
        {
            IEnumerable<ActivationMethod> cached = HttpRuntime.Cache[cacheGetActivationMethods] as IEnumerable<ActivationMethod>;

            if (null == cached)
            {
                cached = repository.GetActivationMethods();
                HttpRuntime.Cache.Insert(cacheGetActivationMethods, cached);
            }

            return cached;
        }

        public static void InvalidateActivationMethodsCaches()
        {
            HttpRuntime.Cache.Remove(cacheGetActivationMethodNames);
            HttpRuntime.Cache.Remove(cacheGetActivationMethods);
        }

        public static IEnumerable<string> GetFeatureNames(CollectorRepository repository)
        {
            IEnumerable<string> cached = HttpRuntime.Cache[cacheGetFeatureNames] as IEnumerable<string>;

            if (null == cached)
            {
                cached = repository.GetFeatureNames();
                HttpRuntime.Cache.Insert(cacheGetFeatureNames, cached);
            }

            return cached;
        }

        public static IEnumerable<Feature> GetFeatures(CollectorRepository repository)
        {
            IEnumerable<Feature> cached = HttpRuntime.Cache[cacheGetFeatures] as IEnumerable<Feature>;

            if (null == cached)
            {
                cached = repository.GetFeatures();
                HttpRuntime.Cache.Insert(cacheGetFeatures, cached);
            }

            return cached;
        }

        public static void InvalidateFeaturesCaches()
        {
            HttpRuntime.Cache.Remove(cacheGetFeatureNames);
            HttpRuntime.Cache.Remove(cacheGetFeatures);
        }

        public static IEnumerable<string> GetExceptionGroupFingerprintHashes(CollectorRepository repository)
        {
            IEnumerable<string> cached = HttpRuntime.Cache[cacheGetExceptionGroupFingerprintHashes] as IEnumerable<string>;

            if (null == cached)
            {
                cached = repository.GetExceptionGroupFingerprintHashes();
                HttpRuntime.Cache.Insert(cacheGetExceptionGroupFingerprintHashes, cached);
            }

            return cached;
        }

        public static IEnumerable<ExceptionGroup> GetExceptionGroups(CollectorRepository repository)
        {
            IEnumerable<ExceptionGroup> cached = HttpRuntime.Cache[cacheGetExceptionGroups] as IEnumerable<ExceptionGroup>;

            if (null == cached)
            {
                cached = repository.GetExceptionGroups();
                HttpRuntime.Cache.Insert(cacheGetExceptionGroups, cached);
            }

            return cached;
        }

        public static void InvalidateExceptionGroupsCaches()
        {
            HttpRuntime.Cache.Remove(cacheGetExceptionGroupFingerprintHashes);
            HttpRuntime.Cache.Remove(cacheGetExceptionGroups);
        }
    }
}
