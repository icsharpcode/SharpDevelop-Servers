using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ICSharpCode.UsageDataCollector.Contracts;

namespace ICSharpCode.UsageDataCollector.ServiceLibrary.Import
{
    public class ExceptionImport
    {
        public string Fingerprint { get; private set; }
        public string Type { get; private set; }
        public string StackTrace { get; private set; }
        public string Location { get; private set; }
        public DateTime Time { get; private set; }

        public ExceptionImport(UsageDataException ude)
            : this(ude.ExceptionType, ude.StackTrace, ude.Time)
        {
        }

        public ExceptionImport(string ExceptionType, string StackTrace, DateTime Time)
        {
            this.Type = Type;
            this.StackTrace = StackTrace;
            this.Time = Time;

            CalculateFingerprint();
            CalculateLocation();

            //TODO: calculate fingerprint hash
        }

        private void CalculateFingerprint()
        {
            this.Fingerprint = this.Type + Environment.NewLine +
                string.Join(
                    Environment.NewLine,
                    ExceptionHelpers.CleanStackTrace(
                        ExceptionHelpers.SplitLines(this.StackTrace)
                    ));
        }

        static readonly Type[] argumentExceptions = { typeof(ArgumentException), typeof(ArgumentNullException), typeof(ArgumentOutOfRangeException) };

        private void CalculateLocation()
        {
            List<string> stackTrace = ExceptionHelpers.SplitLines(this.Fingerprint).Skip(1).ToList();
            // ignore any ThrowHelper (etc.) methods at the top of the stack
            if (stackTrace.Count > 0 && GetFunctionName(stackTrace[0]).Contains("Throw"))
                stackTrace.RemoveAt(0);

            if (stackTrace.Count == 0)
                this.Location = "unknown";

            string type = this.Type;
            if (argumentExceptions.Any(e => e.FullName == type) && ExceptionHelpers.IsUserCode(stackTrace[0]))
            {
                // find first stack frame supplying the invalid argument
                string functionName = GetFunctionName(stackTrace[0]);
                string result = stackTrace.FirstOrDefault(l => GetFunctionName(l) != functionName);
                // report it if it's user code
                if (result != null && ExceptionHelpers.IsUserCode(result))
                    this.Location = result;
                else
                    this.Location = stackTrace[0];
            }
            else
            {
                // report first user-code stack frame
                this.Location = stackTrace.FirstOrDefault(ExceptionHelpers.IsUserCode) ?? stackTrace[0];
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
