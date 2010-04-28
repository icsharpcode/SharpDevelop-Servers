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
        public string FingerprintHash { get; private set; }
        public string Type { get; private set; }
        public string StackTrace { get; private set; }
        public string Location { get; private set; }
        public DateTime Time { get; private set; }

        public long ClientSessionId { get; set; }
        public bool IsFirstInSession { get; set; }

        public ExceptionImport(UsageDataException ude)
            : this(ude.ExceptionType, ude.StackTrace, ude.Time)
        {
        }

        public ExceptionImport(string ExceptionType, string StackTrace, DateTime Time)
        {
            this.Type = ExceptionType;
            this.StackTrace = StackTrace;
            this.Time = Time;

            CalculateFingerprint();
            CalculateLocation();

            this.FingerprintHash = ExceptionHelpers.CalculateFingerprintHash(this.Fingerprint);
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
            if (stackTrace.Count > 0 && ExceptionHelpers.GetFunctionName(stackTrace[0]).Contains("Throw"))
                stackTrace.RemoveAt(0);

            if (0 == stackTrace.Count)
            {
                this.Location = "unknown";
                return;
            }

            string type = this.Type;
            if (argumentExceptions.Any(e => e.FullName == type) && ExceptionHelpers.IsUserCode(stackTrace[0]))
            {
                // find first stack frame supplying the invalid argument
                string functionName = ExceptionHelpers.GetFunctionName(stackTrace[0]);
                string result = stackTrace.FirstOrDefault(l => ExceptionHelpers.GetFunctionName(l) != functionName);
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
    }
}
