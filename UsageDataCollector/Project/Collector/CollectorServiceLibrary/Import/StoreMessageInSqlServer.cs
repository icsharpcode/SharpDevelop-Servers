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
                Session modelSession = new Session()
                {
                    ClientSessionId = msgSession.SessionID,
                    StartTime = msgSession.StartTime,
                    EndTime = msgSession.EndTime,
                    UserId = userId
                };

                newSessions.Add(modelSession);
                repository.Context.Sessions.AddObject(modelSession);
            }

            repository.Context.SaveChanges(); // Save #2

            List<EnvironmentDataName> storedEnvNames = repository.GetEnvironmentDataNames().ToList(); // cacheable

            var insertEnvProperties = (from s in message.Sessions
                                       from prop in s.EnvironmentProperties
                                       join envName in storedEnvNames on prop.Name equals envName.Name
                                       join storedSession in newSessions on s.SessionID equals storedSession.ClientSessionId
                                       select new EnvironmentData()
                                       {
                                           SessionId = storedSession.Id,
                                           EnvironmentDataNameId = envName.Id,
                                           EnvironmentDataValue = prop.Value
                                       });

            foreach (var ede in insertEnvProperties)
                repository.Context.EnvironmentDatas.AddObject(ede);

            List<ActivationMethod> storedActivationMethods = repository.GetActivationMethods().ToList(); // cacheable
            List<Feature> storedFeatures = repository.GetFeatures().ToList(); // cacheable

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
                List<ExceptionGroup> storedExGroups = repository.GetExceptionGroups().ToList(); // cacheable

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
                List<string> knownDataNames = repository.GetEnvironmentDataNameNames().ToList(); // cacheable
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
                List<string> knownActivationMethods = repository.GetActivationMethodNames().ToList(); // cacheable
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
                List<string> knownFeatures = repository.GetFeatureNames().ToList(); // cacheable
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

            List<string> knownExceptionGroups = repository.GetExceptionGroupFingerprintHashes().ToList(); // cacheable
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
            }

            this.denormalisedExceptions = exceptions;
        }
    }
}
