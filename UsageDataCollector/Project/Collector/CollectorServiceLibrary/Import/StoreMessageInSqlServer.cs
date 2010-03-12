using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using ICSharpCode.UsageDataCollector.Contracts;
using ICSharpCode.UsageDataCollector.DataAccess.Collector;

namespace ICSharpCode.UsageDataCollector.ServiceLibrary.Import
{
    public class StoreMessageInSqlServer
    {
        UsageDataMessage message = null;
        CollectorRepository repository = null;
        List<ExceptionImport> denormalisedExceptions = null;

        public StoreMessageInSqlServer(UsageDataMessage msg, CollectorRepository repo)
        {
            message = msg;
            repository = repo;
        }

        // we intentionally don't build the full model in memory first (user -> sessions -> data tables)
        // avoiding concurrency issues (eg type tables) is more important than fewer database writes
        public void ProcessMessage()
        {
            string userGuid = message.UserID.ToString();
            if (String.IsNullOrEmpty(userGuid))
            {
                return;
            }

            PreProcessTypes();

            int userId = repository.FindOrInsertUserByGuid(userGuid);

            List<Session> newSessions = new List<Session>();
            foreach (UsageDataSession msgSession in message.Sessions)
            {
                UsageDataEnvironmentProperty appVersion = msgSession.EnvironmentProperties.FirstOrDefault(ep => ep.Name == "appVersion");
                int appVersionMajor = 0, appVersionMinor = 0, appVersionBuild = 0, appVersionRevision = 0;

                if (null != appVersion && !String.IsNullOrEmpty(appVersion.Value))
                {
                    try
                    {
                        Version v = new Version(appVersion.Value);  // this can throw a couple of exceptions, but we don't care if it does
                        appVersionMajor = v.Major;
                        appVersionMinor = v.Minor;
                        appVersionBuild = v.Build;
                        appVersionRevision = v.Revision;
                    }
                    catch(System.Exception)
                    {
                    }
                }

                Session modelSession = new Session()
                {
                    ClientSessionId = msgSession.SessionID,
                    StartTime = msgSession.StartTime,
                    EndTime = msgSession.EndTime,
                    UserId = userId,
                    AppVersionMajor = appVersionMajor,
                    AppVersionMinor = appVersionMinor,
                    AppVersionBuild = appVersionBuild,
                    AppVersionRevision = appVersionRevision
                };

                newSessions.Add(modelSession);
                repository.Context.Sessions.AddObject(modelSession);
            }

            repository.Context.SaveChanges(); // Save #2

            List<EnvironmentDataName> storedEnvNames = ImportCache.GetEnvironmentDataNames(repository);
            List<EnvironmentDataValue> storedEnvValues = ImportCache.GetEnvironmentDataValues(repository);

            var insertEnvProperties = (from s in message.Sessions
                                       from prop in s.EnvironmentProperties
                                       join envName in storedEnvNames on prop.Name equals envName.Name
                                       join envValue in storedEnvValues on prop.Value equals envValue.Name
                                       join storedSession in newSessions on s.SessionID equals storedSession.ClientSessionId
                                       select new EnvironmentData()
                                       {
                                           SessionId = storedSession.Id,
                                           EnvironmentDataNameId = envName.Id,
                                           EnvironmentDataValueId = envValue.Id
                                       });

            foreach (var ede in insertEnvProperties)
                repository.Context.EnvironmentDatas.AddObject(ede);

            List<ActivationMethod> storedActivationMethods = ImportCache.GetActivationMethods(repository);
            List<Feature> storedFeatures = ImportCache.GetFeatures(repository);

            var insertFeatureUse = (from s in message.Sessions
                                    from fu in s.FeatureUses
                                    join f in storedFeatures on fu.FeatureName equals f.Name
                                    join am in storedActivationMethods on fu.ActivationMethod equals am.Name
                                    join storedSession in newSessions on s.SessionID equals storedSession.ClientSessionId
                                    select new ICSharpCode.UsageDataCollector.DataAccess.Collector.FeatureUse()
                         {
                             ActivationMethodId = am.Id,
                             FeatureId = f.Id,
                             SessionId = storedSession.Id,
                             UseTime = fu.Time,
                             EndTime = fu.EndTime
                         });

            foreach (var fue in insertFeatureUse)
                repository.Context.FeatureUses.AddObject(fue);

            if (null != denormalisedExceptions)
            {
                List<ExceptionGroup> storedExGroups = ImportCache.GetExceptionGroups(repository);

                var insertExceptions = (from e in denormalisedExceptions
                                        join g in storedExGroups on e.FingerprintHash equals g.TypeFingerprintSha256Hash
                                        join storedSession in newSessions on e.ClientSessionId equals storedSession.ClientSessionId
                                        select new ICSharpCode.UsageDataCollector.DataAccess.Collector.Exception()
                                        {
                                            ExceptionGroupId = g.Id,
                                            SessionId = storedSession.Id,
                                            Stacktrace = e.StackTrace,
                                            ThrownAt = e.Time,
                                            IsFirstInSession = e.IsFirstInSession
                                        });

                foreach (var e in insertExceptions)
                    repository.Context.Exceptions.AddObject(e);
            }

            repository.Context.SaveChanges(); // Save #3

        }

        private void PreProcessTypes()
        {
            // Preprocessing of type tables (don't insert any usage data unless type updates went through properly)
            PreProcessEnvironmentDataNames();
            PreProcessEnvironmentDataValues();
            PreProcessActivationMethods();
            PreProcessFeatures();
            PreProcessExceptions();
        }

        protected void PreProcessEnvironmentDataNames()
        {
            List<string> distinctMsgEnvProperties = (from s in message.Sessions
                                                     from p in s.EnvironmentProperties
                                                     select p.Name).Distinct().ToList();

            // did we receive environment data at all?
            if (distinctMsgEnvProperties.Count > 0)
            {
                List<string> knownDataNames = ImportCache.GetEnvironmentDataNameNames(repository);
                List<string> missing = distinctMsgEnvProperties.Except(knownDataNames).ToList();

                // this happens rarely for environment data names
                if (missing.Count > 0)
                {
                    foreach (string envdn in missing)
                    {
                        EnvironmentDataName modelEdn = new EnvironmentDataName()
                        {
                            Name = envdn
                        };

                        repository.Context.EnvironmentDataNames.AddObject(modelEdn);
                    }

                    repository.IgnoreDuplicateKeysOnSaveChanges<EnvironmentDataName>();
                    ImportCache.InvalidateEnvironmentDataNamesCaches();
                }
            }
        }

        protected void PreProcessEnvironmentDataValues()
        {
            List<string> distinctMsgEnvValues = (from s in message.Sessions
                                                   from p in s.EnvironmentProperties
                                                   select p.Value).Distinct().ToList();

            if (distinctMsgEnvValues.Count > 0)
            {
                List<string> knownDataValueNames = ImportCache.GetEnvironmentDataValueNames(repository);
                List<string> missing = distinctMsgEnvValues.Except(knownDataValueNames).ToList();

                if (missing.Count > 0)
                {
                    foreach (string vn in missing)
                    {
                        EnvironmentDataValue modelValue = new EnvironmentDataValue()
                        {
                            Name = vn
                        };

                        repository.Context.EnvironmentDataValues.AddObject(modelValue);
                    }

                    repository.IgnoreDuplicateKeysOnSaveChanges<EnvironmentDataValue>();
                    ImportCache.InvalidateEnvironmentDataValueCaches();
                }
            }
        }

        protected void PreProcessActivationMethods()
        {
            List<string> distinctMsgActivationMethods = (from s in message.Sessions
                                                         from fu in s.FeatureUses
                                                         select fu.ActivationMethod).Distinct().ToList();

            if (distinctMsgActivationMethods.Count > 0)
            {
                List<string> knownActivationMethods = ImportCache.GetActivationMethodNames(repository);
                List<string> missing = distinctMsgActivationMethods.Except(knownActivationMethods).ToList();

                if (missing.Count > 0)
                {
                    foreach (string am in missing)
                    {
                        ActivationMethod modelAM = new ActivationMethod()
                        {
                            Name = am
                        };

                        repository.Context.ActivationMethods.AddObject(modelAM);
                    }

                    repository.IgnoreDuplicateKeysOnSaveChanges<ActivationMethod>();
                    ImportCache.InvalidateActivationMethodsCaches();
                }
            }
        }

        protected void PreProcessFeatures()
        {
            List<string> distinctMsgFeatures = (from s in message.Sessions
                                                from fu in s.FeatureUses
                                                select fu.FeatureName).Distinct().ToList();

            if (distinctMsgFeatures.Count > 0)
            {
                List<string> knownFeatures = ImportCache.GetFeatureNames(repository);
                List<string> missing = distinctMsgFeatures.Except(knownFeatures).ToList();

                if (missing.Count > 0)
                {
                    foreach (string fn in missing)
                    {
                        Feature modelFeature = new Feature()
                        {
                            Name = fn
                        };

                        repository.Context.Features.AddObject(modelFeature);
                    }

                    repository.IgnoreDuplicateKeysOnSaveChanges<Feature>();
                    ImportCache.InvalidateFeaturesCaches();
                }
            }
        }

        protected void PreProcessExceptions()
        {
            List<ExceptionImport> exceptions = (from s in message.Sessions
                                                from e in s.Exceptions
                                                select new ExceptionImport(e)
                                                {
                                                    ClientSessionId = s.SessionID,
                                                    IsFirstInSession = false
                                                }).ToList();

            if (0 == exceptions.Count) return; // no exceptions reported, denormalisedExceptions remains null

            // mark first exception in session - note that above LINQ query does not mix up order of items
            long clientSession = -1;
            exceptions.ForEach(ex =>
            {
                if (ex.ClientSessionId != clientSession)
                {
                    ex.IsFirstInSession = true;
                    clientSession = ex.ClientSessionId;
                }
            });

            List<string> distinctMsgExceptionGroups = (from e in exceptions
                                                       select e.FingerprintHash).Distinct().ToList();

            List<string> knownExceptionGroups = ImportCache.GetExceptionGroupFingerprintHashes(repository);
            List<string> missing = distinctMsgExceptionGroups.Except(knownExceptionGroups).ToList();

            if (missing.Count > 0)
            {
                List<ExceptionImport> groupsToAdd = (from e in exceptions
                                                     join m in missing on e.FingerprintHash equals m
                                                     select e).Distinct(new ExceptionImportGroupEqualityComparer()).ToList();

                foreach (ExceptionImport imp in groupsToAdd)
                {
                    ExceptionGroup modelGroup = new ExceptionGroup()
                    {
                        ExceptionFingerprint = imp.Fingerprint,
                        ExceptionLocation = imp.Location,
                        ExceptionType = imp.Type,
                        TypeFingerprintSha256Hash = imp.FingerprintHash
                    };

                    repository.Context.ExceptionGroups.AddObject(modelGroup);
                }

                repository.IgnoreDuplicateKeysOnSaveChanges<ExceptionGroup>();
                ImportCache.InvalidateExceptionGroupsCaches();
            }

            this.denormalisedExceptions = exceptions;
        }
    }
}
