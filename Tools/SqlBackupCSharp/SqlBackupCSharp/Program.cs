using System;
using System.Collections.Generic;
using Microsoft.SqlServer.Management.Common;
using Microsoft.SqlServer.Management.Smo;

namespace SqlBackupCSharp
{
    class Program
    {
        public static string BaseBackupLocation = @"z:\db\";

        static void Main(string[] args)
        {
            List<BackupInformation> backups = new List<BackupInformation>()
            {
                new BackupInformation() { Server = @".\dev", Database ="rft" },
                new BackupInformation() { Server = @".\dev", Database ="sdrwiki" },
                new BackupInformation() { Server = @".\dev", Database ="sharpdevelopwiki" },
                new BackupInformation() { Server = @".\dev", Database ="gemini" },
                new BackupInformation() { Server = @".\community", Database ="cs" }
            };

            foreach (BackupInformation b in backups)
            {
                PerformBackup(b);
            }

            Console.WriteLine("All backups complete");
        }

        public static void PerformBackup(BackupInformation b)
        {
            string backupDeviceName = BaseBackupLocation + 
                                      b.Database +
                                      DateTime.Now.Date.ToString("yyyyMMdd") + 
                                      ".bak";

            BackupDeviceItem bdi =
                new BackupDeviceItem(backupDeviceName, DeviceType.File);

            Backup bu = new Backup();
            bu.Database = b.Database;
            bu.Devices.Add(bdi);
            bu.Initialize = true;

            // add percent complete handler
            // bu.PercentComplete += new PercentCompleteEventHandler(Backup_PercentComplete);

            // add complete event handler
            bu.Complete += new ServerMessageEventHandler(Backup_Complete);

            Server server = new Server(b.Server);
            bu.SqlBackup(server);
        }

        protected static void Backup_PercentComplete(object sender, PercentCompleteEventArgs e)
        {
            Console.WriteLine(e.Percent + "% processed.");
        }

        protected static void Backup_Complete(object sender, ServerMessageEventArgs e)
        {
            Console.WriteLine(Environment.NewLine + e.ToString());
        }
    }
}