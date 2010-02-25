using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ICSharpCode.UsageDataCollector.ServiceLibrary.Import
{
    public class ExceptionImportGroupEqualityComparer : IEqualityComparer<ExceptionImport>
    {
        public bool Equals(ExceptionImport x, ExceptionImport y)
        {
            if (x.FingerprintHash == y.FingerprintHash)
                return true;

            return false;
        }

        public int GetHashCode(ExceptionImport obj)
        {
            return obj.FingerprintHash.GetHashCode();
        }
    }
}
