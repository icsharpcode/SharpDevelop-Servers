using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ICSharpCode.UsageDataCollector.ServiceLibrary.Import
{
    public class BulkImport
    {
        public void SketchOut()
        {
            // list directory

            // etl each file separately
                // load file
                // match all four types & insert (if necessary); match in memory (performance)
                // match user & insert (if necessary); via sproc (Cache issue)
                // load session and values into database


            // delete file when done & no exceptions

            // done
        }
    }
}
