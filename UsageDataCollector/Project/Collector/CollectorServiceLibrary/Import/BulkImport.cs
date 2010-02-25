using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using ICSharpCode.UsageDataCollector.Contracts;
using ICSharpCode.UsageDataCollector.DataAccess.Collector;

namespace ICSharpCode.UsageDataCollector.ServiceLibrary.Import
{
    public class BulkImport
    {
        // list directory
        // etl each file separately
            // load file
            // match all four types & insert (if necessary); match in memory (performance)
            // match user & insert (if necessary); via sproc (Cache issue)
            // load session and values into database
        // delete file when done & no exceptions
        public static void SketchOut()
        {
            // wrong schema errors by moving contracts to /contracts namespace
            UsageDataMessage message =
                FileImporter.ReadMessage(@"D:\Daten\SharpDevelop\trunk\SharpDevelopServers\UsageDataCollector\SampleData\_Debugger_Exception_ab7a92f4-3d0e-44ac-afc9-a4d6090603b0.xml.gz");

            using (var context = CollectorRepository.CreateContext())
            {
                CollectorRepository repo = new CollectorRepository();
                repo.Context = context;

                CrackAndStoreMessage processor = new CrackAndStoreMessage(message, repo);
                processor.ProcessMessage();


                // Dictionary<string,int> features = context.Features.ToDictionary(f => f.Name, f => f.Id);
                // var features = context.Features.ToList().AsReadOnly();
                // features: a, b, c
                // usage features: b, c, d --> find d
                // List<string> knownFeatures = context.Features.Select(f => f.Name).ToList();
            }
        }
    }
}
