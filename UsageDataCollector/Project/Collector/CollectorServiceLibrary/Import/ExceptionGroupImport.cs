using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ICSharpCode.UsageDataCollector.ServiceLibrary.Import
{
    sealed class ExceptionGroupImport
    {
        public string Fingerprint;

        public string CrashID
        {
            get { return unchecked((uint)this.Fingerprint.GetHashCode() % 10000u).ToString("d4"); }
        }

        public string Type
        {
            get
            {
                return ExceptionHelpers.SplitLines(this.Fingerprint).First();
            }
        }

        static readonly Type[] argumentExceptions = { typeof(ArgumentException), typeof(ArgumentNullException), typeof(ArgumentOutOfRangeException) };

        public string Location
        {
            get
            {
                List<string> stackTrace = ExceptionHelpers.SplitLines(this.Fingerprint).Skip(1).ToList();
                // ignore any ThrowHelper (etc.) methods at the top of the stack
                if (stackTrace.Count > 0 && GetFunctionName(stackTrace[0]).Contains("Throw"))
                    stackTrace.RemoveAt(0);

                if (stackTrace.Count == 0)
                    return "unknown";
                string type = this.Type;
                if (argumentExceptions.Any(e => e.FullName == type) && ExceptionHelpers.IsUserCode(stackTrace[0]))
                {
                    // find first stack frame supplying the invalid argument
                    string functionName = GetFunctionName(stackTrace[0]);
                    string result = stackTrace.FirstOrDefault(l => GetFunctionName(l) != functionName);
                    // report it if it's user code
                    if (result != null && ExceptionHelpers.IsUserCode(result))
                        return result;
                    else
                        return stackTrace[0];
                }
                else
                {
                    // report first user-code stack frame
                    return stackTrace.FirstOrDefault(ExceptionHelpers.IsUserCode) ?? stackTrace[0];
                }
            }
        }

        static string GetFunctionName(string stackTraceLine)
        {
            int pos = stackTraceLine.IndexOf('(');
            if (pos > 0)
                return stackTraceLine.Substring(0, pos);
            else
                return stackTraceLine;
        }
    }
}
