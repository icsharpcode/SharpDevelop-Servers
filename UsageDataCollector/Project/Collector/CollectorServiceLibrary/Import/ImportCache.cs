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
        private static string cacheGetEnvironmentDataValueNames = "GetEnvironmentDataValueNames";
        private static string cacheGetEnvironmentDataValues = "GetEnvironmentDataValues";
        

        public static List<string> GetEnvironmentDataNameNames(CollectorRepository repository)
        {
            List<string> cached = HttpRuntime.Cache[cacheGetEnvironmentDataNameNames] as List<string>;

            if (null == cached)
            {
                cached = repository.GetEnvironmentDataNameNames().ToList();
                HttpRuntime.Cache.Insert(cacheGetEnvironmentDataNameNames, cached);
            }

            return cached;
        }

        public static List<EnvironmentDataName> GetEnvironmentDataNames(CollectorRepository repository)
        {
            List<EnvironmentDataName> cached = HttpRuntime.Cache[cacheGetEnvironmentDataNames] as List<EnvironmentDataName>;

            if (null == cached)
            {
                cached = repository.GetEnvironmentDataNames().ToList();
                HttpRuntime.Cache.Insert(cacheGetEnvironmentDataNames, cached);
            }

            return cached;
        }

        public static void InvalidateEnvironmentDataNamesCaches()
        {
            HttpRuntime.Cache.Remove(cacheGetEnvironmentDataNameNames);
            HttpRuntime.Cache.Remove(cacheGetEnvironmentDataNames);
        }

        public static List<string> GetActivationMethodNames(CollectorRepository repository)
        {
            List<string> cached = HttpRuntime.Cache[cacheGetActivationMethodNames] as List<string>;

            if (null == cached)
            {
                cached = repository.GetActivationMethodNames().ToList();
                HttpRuntime.Cache.Insert(cacheGetActivationMethodNames, cached);
            }

            return cached;
        }

        public static List<ActivationMethod> GetActivationMethods(CollectorRepository repository)
        {
            List<ActivationMethod> cached = HttpRuntime.Cache[cacheGetActivationMethods] as List<ActivationMethod>;

            if (null == cached)
            {
                cached = repository.GetActivationMethods().ToList();
                HttpRuntime.Cache.Insert(cacheGetActivationMethods, cached);
            }

            return cached;
        }

        public static void InvalidateActivationMethodsCaches()
        {
            HttpRuntime.Cache.Remove(cacheGetActivationMethodNames);
            HttpRuntime.Cache.Remove(cacheGetActivationMethods);
        }

        public static List<string> GetFeatureNames(CollectorRepository repository)
        {
            List<string> cached = HttpRuntime.Cache[cacheGetFeatureNames] as List<string>;

            if (null == cached)
            {
                cached = repository.GetFeatureNames().ToList();
                HttpRuntime.Cache.Insert(cacheGetFeatureNames, cached);
            }

            return cached;
        }

        public static List<Feature> GetFeatures(CollectorRepository repository)
        {
            List<Feature> cached = HttpRuntime.Cache[cacheGetFeatures] as List<Feature>;

            if (null == cached)
            {
                cached = repository.GetFeatures().ToList();
                HttpRuntime.Cache.Insert(cacheGetFeatures, cached);
            }

            return cached;
        }

        public static void InvalidateFeaturesCaches()
        {
            HttpRuntime.Cache.Remove(cacheGetFeatureNames);
            HttpRuntime.Cache.Remove(cacheGetFeatures);
        }

        public static List<string> GetExceptionGroupFingerprintHashes(CollectorRepository repository)
        {
            List<string> cached = HttpRuntime.Cache[cacheGetExceptionGroupFingerprintHashes] as List<string>;

            if (null == cached)
            {
                cached = repository.GetExceptionGroupFingerprintHashes().ToList();
                HttpRuntime.Cache.Insert(cacheGetExceptionGroupFingerprintHashes, cached);
            }

            return cached;
        }

        public static List<ExceptionGroup> GetExceptionGroups(CollectorRepository repository)
        {
            List<ExceptionGroup> cached = HttpRuntime.Cache[cacheGetExceptionGroups] as List<ExceptionGroup>;

            if (null == cached)
            {
                cached = repository.GetExceptionGroups().ToList();
                HttpRuntime.Cache.Insert(cacheGetExceptionGroups, cached);
            }

            return cached;
        }

        public static void InvalidateExceptionGroupsCaches()
        {
            HttpRuntime.Cache.Remove(cacheGetExceptionGroupFingerprintHashes);
            HttpRuntime.Cache.Remove(cacheGetExceptionGroups);
        }

        public static List<string> GetEnvironmentDataValueNames(CollectorRepository repository)
        {
            List<string> cached = HttpRuntime.Cache[cacheGetEnvironmentDataValueNames] as List<string>;

            if (null == cached)
            {
                cached = repository.GetEnvironmentDataValueNames().ToList();
                HttpRuntime.Cache.Insert(cacheGetEnvironmentDataValueNames, cached);
            }

            return cached;
        }

        public static List<EnvironmentDataValue> GetEnvironmentDataValues(CollectorRepository repository)
        {
            List<EnvironmentDataValue> cached = HttpRuntime.Cache[cacheGetEnvironmentDataValues] as List<EnvironmentDataValue>;

            if (null == cached)
            {
                cached = repository.GetEnvironmentDataValues().ToList();
                HttpRuntime.Cache.Insert(cacheGetEnvironmentDataValues, cached);
            }

            return cached;
        }

        public static void InvalidateEnvironmentDataValueCaches()
        {
            HttpRuntime.Cache.Remove(cacheGetEnvironmentDataValueNames);
            HttpRuntime.Cache.Remove(cacheGetEnvironmentDataValues);
        }
    }
}
