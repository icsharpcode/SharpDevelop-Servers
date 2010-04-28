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
            // ImportMessagesToSqlServer.ImportSingleMessage(@"D:\Temp\UDC_ImportTest\failed\c49e9493-c941-4cdd-996d-8596540fb0a7.xml.gz");

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
