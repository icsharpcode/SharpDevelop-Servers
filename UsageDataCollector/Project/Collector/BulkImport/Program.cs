using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using ICSharpCode.UsageDataCollector.ServiceLibrary.Import;
using System.IO;

namespace BulkImporter
{
    class Program
    {
        static void Main(string[] args)
        {
            if (args.Length != 1)
            {
                ShowUsage();
                return;
            }

            string messageDirectory = args[0];

            if (!Directory.Exists(messageDirectory))
            {
                Console.WriteLine("Error: Directory does not exist");
                return;
            }

            // -- uncomment following line for simple testing purposes only
            // ImportMessagesToSqlServer.ImportSingleMessage(@"D:\Daten\SharpDevelop\trunk\SharpDevelopServers\UsageDataCollector\SampleData\_Debugger_Exception_ab7a92f4-3d0e-44ac-afc9-a4d6090603b0.xml.gz");

            ImportMessagesToSqlServer.ImportMessagesFromDirectory(messageDirectory, true);
        }

        private static void ShowUsage()
        {
            Console.WriteLine();
            Console.WriteLine("Usage: bulkimport <path to directory>");
            Console.WriteLine();
            Console.WriteLine("Connection string is set via app.config");
            Console.WriteLine();
        }
    }
}
