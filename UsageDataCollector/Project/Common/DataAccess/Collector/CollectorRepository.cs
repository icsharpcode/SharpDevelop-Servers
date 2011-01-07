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

        public static UDCContext CreateContext(string connectionString)
        {
            return new UDCContext(connectionString);
        }

        public User FindUserByGuid(string guid)
        {
            return Context.Users.FirstOrDefault(u => u.AssociatedGuid == guid);
        }

        public int FindOrInsertUserByGuid(string guid)
        {
            User modelUser = FindUserByGuid(guid);

            if (null == modelUser)
            {
                modelUser = new User()
                {
                    AssociatedGuid = guid
                };

                Context.Users.AddObject(modelUser);

                try
                {
                    Context.SaveChanges();
                }
                catch (System.Data.UpdateException)
                {
                    Context.Users.Detach(modelUser);

                    modelUser = FindUserByGuid(guid); // find again (only very, very rare cases will exhibit this dual-search)
                }
            }

            if (null != modelUser)
                return modelUser.Id;

            return -1;
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

        public IEnumerable<string> GetExceptionGroupFingerprintHashes()
        {
            return Context.ExceptionGroups.Select(eg => eg.TypeFingerprintSha256Hash);
        }

        public IEnumerable<ExceptionGroup> GetExceptionGroups()
        {
            return Context.ExceptionGroups.Select(g => g);
        }

        public IEnumerable<string> GetEnvironmentDataValueNames()
        {
            return Context.EnvironmentDataValues.Select(d => d.Name);
        }

        public IEnumerable<EnvironmentDataValue> GetEnvironmentDataValues()
        {
            return Context.EnvironmentDataValues.Select(d => d);
        }

        //        System.Data.UpdateException was unhandled
        //  Message=An error occurred while updating the entries. See the inner exception for details.
        //  Source=System.Data.Entity
        //  StackTrace:
        //       at System.Data.Mapping.Update.Internal.UpdateTranslator.Update(IEntityStateManager stateManager, IEntityAdapter adapter)
        //       at System.Data.EntityClient.EntityAdapter.Update(IEntityStateManager entityCache)
        //       at System.Data.Objects.ObjectContext.SaveChanges(SaveOptions options)
        //       at System.Data.Objects.ObjectContext.SaveChanges()
        //       at ICSharpCode.UsageDataCollector.ServiceLibrary.Import.CrackAndStoreMessage.PreProcessEnvironmentDataNames() in D:\Daten\SharpDevelop\trunk\SharpDevelopServers\UsageDataCollector\Project\Collector\CollectorServiceLibrary\Import\CrackAndStoreMessage.cs:line 153
        //       at ICSharpCode.UsageDataCollector.ServiceLibrary.Import.CrackAndStoreMessage.ProcessMessage() in D:\Daten\SharpDevelop\trunk\SharpDevelopServers\UsageDataCollector\Project\Collector\CollectorServiceLibrary\Import\CrackAndStoreMessage.cs:line 34
        //       at ICSharpCode.UsageDataCollector.ServiceLibrary.Import.BulkImport.SketchOut() in D:\Daten\SharpDevelop\trunk\SharpDevelopServers\UsageDataCollector\Project\Collector\CollectorServiceLibrary\Import\BulkImport.cs:line 33
        //       at BulkImporter.Program.Main(String[] args) in D:\Daten\SharpDevelop\trunk\SharpDevelopServers\UsageDataCollector\Project\Collector\BulkImport\Program.cs:line 14
        //       at System.AppDomain._nExecuteAssembly(RuntimeAssembly assembly, String[] args)
        //       at System.AppDomain.ExecuteAssembly(String assemblyFile, Evidence assemblySecurity, String[] args)
        //       at Microsoft.VisualStudio.HostingProcess.HostProc.RunUsersAssembly()
        //       at System.Threading.ThreadHelper.ThreadStart_Context(Object state)
        //       at System.Threading.ExecutionContext.Run(ExecutionContext executionContext, ContextCallback callback, Object state, Boolean ignoreSyncCtx)
        //       at System.Threading.ExecutionContext.Run(ExecutionContext executionContext, ContextCallback callback, Object state)
        //       at System.Threading.ThreadHelper.ThreadStart()
        //  InnerException: System.Data.SqlClient.SqlException
        //       Message=Cannot insert duplicate key row in object 'dbo.EnvironmentDataNames' with unique index 'IX_EnvironmentDataNames'.
        //The statement has been terminated.
        //       Source=.Net SqlClient Data Provider
        //       ErrorCode=-2146232060
        //       Class=14
        //       LineNumber=1
        //       Number=2601
        //       Procedure=""
        //       Server=localhost
        //       State=1
        //       StackTrace:
        //            at System.Data.SqlClient.SqlConnection.OnError(SqlException exception, Boolean breakConnection)
        //            at System.Data.SqlClient.SqlInternalConnection.OnError(SqlException exception, Boolean breakConnection)
        //            at System.Data.SqlClient.TdsParser.ThrowExceptionAndWarning()
        //            at System.Data.SqlClient.TdsParser.Run(RunBehavior runBehavior, SqlCommand cmdHandler, SqlDataReader dataStream, BulkCopySimpleResultSet bulkCopyHandler, TdsParserStateObject stateObj)
        //            at System.Data.SqlClient.SqlDataReader.ConsumeMetaData()
        //            at System.Data.SqlClient.SqlDataReader.get_MetaData()
        //            at System.Data.SqlClient.SqlCommand.FinishExecuteReader(SqlDataReader ds, RunBehavior runBehavior, String resetOptionsString)
        //            at System.Data.SqlClient.SqlCommand.RunExecuteReaderTds(CommandBehavior cmdBehavior, RunBehavior runBehavior, Boolean returnStream, Boolean async)
        //            at System.Data.SqlClient.SqlCommand.RunExecuteReader(CommandBehavior cmdBehavior, RunBehavior runBehavior, Boolean returnStream, String method, DbAsyncResult result)
        //            at System.Data.SqlClient.SqlCommand.RunExecuteReader(CommandBehavior cmdBehavior, RunBehavior runBehavior, Boolean returnStream, String method)
        //            at System.Data.SqlClient.SqlCommand.ExecuteReader(CommandBehavior behavior, String method)
        //            at System.Data.SqlClient.SqlCommand.ExecuteDbDataReader(CommandBehavior behavior)
        //            at System.Data.Common.DbCommand.ExecuteReader(CommandBehavior behavior)
        //            at System.Data.Mapping.Update.Internal.DynamicUpdateCommand.Execute(UpdateTranslator translator, EntityConnection connection, Dictionary`2 identifierValues, List`1 generatedValues)
        //            at System.Data.Mapping.Update.Internal.UpdateTranslator.Update(IEntityStateManager stateManager, IEntityAdapter adapter)
        //       InnerException: 
        public void IgnoreDuplicateKeysOnSaveChanges<T>()
        {
            try
            {
                Context.SaveChanges();
            }
            catch (System.Data.UpdateException ue)
            {
                int stateEntryCount = ue.StateEntries.Count();

                foreach (var se in ue.StateEntries)
                    Context.Detach((T)se.Entity);

                IgnoreDuplicateKeysOnSaveChanges<T>();   // there could be more exceptions
            }
        }

        public TaggedCommit GetTag(string name, bool isRelease)
        {
            return Context.TaggedCommits.FirstOrDefault(tag => tag.Name == name && tag.IsRelease == isRelease);
        }

        public Commit GetCommitByHash(string hash)
        {
            return Context.Commits.FirstOrDefault(c => c.Hash == hash);
        }
    }
}
