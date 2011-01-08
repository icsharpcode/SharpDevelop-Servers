using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Security.Cryptography;

namespace ICSharpCode.UsageDataCollector.ServiceLibrary.Import
{
    public class ExceptionHelpers
    {
        public static IEnumerable<string> CleanStackTrace(IEnumerable<string> input)
        {
            bool inUserCode = false;
            int linesInUserCode = 0;
            int linesTotal = 0;
            string lastLine = null;
            foreach (string line in input)
            {
                string cleaned = CleanStackTraceLine(line);
                if (cleaned == lastLine) // ignore number of calls for recursive functions
                    continue;
                lastLine = cleaned;

                if (IsUserCode(cleaned))
                    inUserCode = true;
                if (inUserCode)
                    linesInUserCode++;
                linesTotal++;
                yield return cleaned;
                if (linesInUserCode >= 3)
                    break;
            }
        }

		public static bool IsUserCode(string cleaned)
		{
			return !(cleaned.StartsWith("Microsoft.", StringComparison.OrdinalIgnoreCase)
				|| cleaned.StartsWith("System.", StringComparison.OrdinalIgnoreCase)
				|| cleaned.StartsWith("MS.", StringComparison.OrdinalIgnoreCase)
				|| cleaned.StartsWith("--", StringComparison.OrdinalIgnoreCase));
		}

        static string CleanStackTraceLine(string text)
        {
            text = text.TrimStart();
            if (text.StartsWith("--"))
                return text;
            int pos = text.IndexOfAny(new char[] { ' ', '\t' });
            if (pos >= 0)
                text = text.Substring(pos + 1);

            pos = text.IndexOf(" in ");
            if (pos < 0)
                return text;
            else
                return text.Substring(0, pos);
        }

        public static IEnumerable<string> SplitLines(string input)
        {
            using (StringReader r = new StringReader(input))
            {
                string line;
                while ((line = r.ReadLine()) != null)
                    yield return line;
            }
        }

        public static string GetFunctionName(string stackTraceLine)
        {
            int pos = stackTraceLine.IndexOf('(');
            if (pos > 0)
                return stackTraceLine.Substring(0, pos);
            else
                return stackTraceLine;
        }

        public static string CalculateFingerprintHash(string fingerprint)
        {
            SHA256Managed sh = new SHA256Managed();
            byte[] request = UTF8Encoding.UTF8.GetBytes(fingerprint);
            
            sh.Initialize();
            byte[] hash = sh.ComputeHash(request, 0, request.Length);

            return Convert.ToBase64String(hash);
        }
    }
}
