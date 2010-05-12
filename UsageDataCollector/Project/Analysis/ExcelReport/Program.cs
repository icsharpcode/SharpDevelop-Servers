using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ExcelReport.DataAccess;
using Microsoft.Office.Interop.Excel;
using System.IO;
using System.Collections.ObjectModel;

namespace ExcelReport
{
    class Program
    {
        const int minimumNumberOfAffectedUsersForReportedExceptions = 2;
        const int maximumInstancesPerException = 10;

        static int Main(string[] args)
        {
            if (args.Length < 1)
            {
                Console.WriteLine("Usage: ExcelReport.exe <outputfilename.xlsx>");
                return 1;
            }
            string targetFileName = args[0];
            if (File.Exists(targetFileName))
                File.Delete(targetFileName);

            using (UsageDataAnalysisEntities context = new UsageDataAnalysisEntities())
            {
                ReportRepository r = new ReportRepository(context);
                r.MinimumDate = new DateTime(2009, 11, 1);
                r.MinimumVersion = new Version(4, 0, 0, 1);
                Console.WriteLine("Loading excel...");
                using (ExcelWorkbook workbook = new ExcelWorkbook())
                {
                    CreateExceptionDetails(workbook, r);
                    CreateExceptionList(workbook, r);
                    CreateEnvironment(workbook, r);
                    CreateUsageSheet(workbook, r);
                    workbook.Save(targetFileName);
                }
            }
            return 0;
        }

        #region CalculateKnownVersions
        static Version[] CalculateKnownVersions(ReportRepository repository)
        {
            const int blocks = 4;
            const int versionsPerBlock = 2;
            
            HashSet<Version> popularVersions = new HashSet<Version>();
            DateTime startDate = repository.Sessions.Min(s => s.StartTime);
            DateTime endDate = repository.Sessions.Max(s => s.StartTime);
            
            TimeSpan span = endDate - startDate;
            // last week has its own block, other 3 blocks are distributed evenly over the remaining timespan
            if (span > new TimeSpan(30, 0, 0, 0))
                span -= new TimeSpan(7, 0, 0, 0);
            
            foreach (var groupByBlock in repository.Sessions.AsEnumerable().GroupBy(s => (s.StartTime.Ticks-startDate.Ticks)*(blocks-1)/span.Ticks).OrderBy(g => g.Key)) {
                int versionsAddedThisBlock = 0;
                foreach (var version in groupByBlock.GroupBy(s => s.AppVersion).OrderByDescending(g => g.Count())) {
                    if (popularVersions.Add(version.Key)) {
                        if (++versionsAddedThisBlock >= versionsPerBlock)
                            break;
                    }
                }
            }
            return popularVersions.OrderBy(v => v).ToArray();
        }
        #endregion
        
        #region Usage
        private static void CreateUsageSheet(ExcelWorkbook workbook, ReportRepository repository)
        {
            Worksheet ws = workbook.CreateSheet("Usage");
            var knownVersions = CalculateKnownVersions(repository);
            UsageTable usageTable = new UsageTable(repository.Sessions, knownVersions);

            int pos = 1;
            int chartPos = 30;
            CreateUsageSheet(ws, knownVersions, ref pos, ref chartPos, usageTable.CreateDaily(), null, "Number of users per day");
            CreateUsageSheet(ws, knownVersions, ref pos, ref chartPos, usageTable.CreateWeekly(), null, "Number of users per week");
            CreateUsageSheet(ws, knownVersions, ref pos, ref chartPos, usageTable.CreateMonthly(), "MM.yyyy", "Number of users per month");
        }

        private static void CreateUsageSheet(Worksheet ws, Version[] knownVersions, ref int pos, ref int chartPos, ReadOnlyCollection<UsageTable.Row> data, string dateFormat, string title)
        {
            ws.Cells[pos++, 1] = title;
            int startpos = pos;
            ws.Cells[pos, 1] = "Date";
            for (int i = 0; i < knownVersions.Length; i++)
                ws.Cells[pos, 2 + i] = knownVersions[i].ToString();
            ws.Cells[pos, 2 + knownVersions.Length] = "Other";
            ws.Columns["A"].ColumnWidth = 10;
            pos++;

            object[,] excelData = new object[data.Count, 2 + knownVersions.Length];
            for (int i = 0; i < data.Count; i++)
            {
                var row = data[i];
                if (dateFormat != null)
                    excelData[i, 0] = "'" + row.Date.ToString(dateFormat);
                else
                    excelData[i, 0] = row.Date;
                for (int j = 0; j < row.Values.Length; j++)
                    excelData[i, j + 1] = row.Values[j];
            }

            ws.Cells[pos, 1].Resize[data.Count, 2 + knownVersions.Length].Value = excelData;
            pos += data.Count;

            var dataRange = ws.Cells[startpos, 1].Resize[data.Count + 1, 2 + knownVersions.Length];

            ChartObjects chartObjects = (ChartObjects)ws.ChartObjects();
            ChartObject newChartObject = chartObjects.Add(250, chartPos, 650, 350);
            chartPos += 400;
            newChartObject.Chart.ChartWizard(dataRange, XlChartType.xlAreaStacked,
                                             PlotBy: XlRowCol.xlColumns,
                                             Title: title);

            pos += 1;
        }
        #endregion

        #region Environment
        static void CreateEnvironment(ExcelWorkbook workbook, ReportRepository repository)
        {
            Worksheet ws = workbook.CreateSheet("Environment");

            // set of available environment properties
            var environmentProperties = repository.EnvironmentDataNames.OrderBy(n => n.Name).ToList();
            
            // take one session per day from each user
            var sessionsQuery = from s in repository.Sessions
                                group s by new { UserID = s.UserId, s.StartTime.Day, s.StartTime.Month, s.StartTime.Year } into g
                                select g.FirstOrDefault();
            
            ChartObjects chartObjects = ws.ChartObjects();

            int pos = 1;
            int chartPos = 30;
            foreach (var property in environmentProperties)
            {
                ws.Cells[pos, 1] = property.Name;
                Range range = ws.Cells[pos, 1];
                range.Font.Size = 14;
                pos++;

                var q = (
                    from s in sessionsQuery
                    let p = s.EnvironmentDatas.FirstOrDefault(p => p.NameId == property.Id)
                    group p by (p != null ? p.EnvironmentDataValue.Value : "unknown") into g
                    select new { Value = g.Key, Count = g.Count() }
                ).ToList();

                // if there are more than 10 values, combine all but 9 into "Other"
                if (q.Count > 10)
                {
                    q.Sort((a, b) => b.Count.CompareTo(a.Count));
                    int otherCount = 0;
                    for (int i = 9; i < q.Count; i++)
                    {
                        otherCount += q[i].Count;
                    }
                    q.RemoveRange(9, q.Count - 9);
                    q.Add(new { Value = "Other", Count = otherCount });
                }

                int startpos = pos;
                object[,] excelData = new object[q.Count, 2];
                foreach (var pair in q.OrderBy(p => p.Value))
                {
                    excelData[pos - startpos, 0] = pair.Value;
                    excelData[pos - startpos, 1] = pair.Count;
                    pos++;
                }

                Range dataRange = ws.Cells[startpos, 1].Resize[pos - startpos, 2];
                dataRange.Value = excelData;

                var newChartObject = chartObjects.Add(250, chartPos, 450, 300);
                chartPos += 350;
                newChartObject.Chart.ChartWizard(dataRange, XlChartType.xlPie, Title: property.Name,
                                                 HasLegend: false, // we use data labels, no legend
                                                 CategoryLabels: 1 /* column is for labels */);
                newChartObject.Chart.ApplyDataLabels(ShowCategoryName: true, ShowValue: false, ShowPercentage: true);

                pos++; // empty line
                Console.Write('.');
            }
            Console.WriteLine();
        }
        #endregion

        #region ExceptionDetails
        static void CreateExceptionDetails(ExcelWorkbook workbook, ReportRepository repository)
        {
            Worksheet ws = workbook.CreateSheet("Exception Details");
            ws.Columns[1].ColumnWidth = 20;
            ws.Columns[1].VerticalAlignment = Constants.xlTop;
            ws.Columns[2].ColumnWidth = 170;
            ws.Columns[2].HorizontalAlignment = Constants.xlLeft;
            ws.Columns[2].VerticalAlignment = Constants.xlTop;

            var query = from session in repository.Sessions
                        from exception in session.Exceptions
                        where exception.IsFirstInSession
                        group new
                        {
                            Time = exception.ThrownAt,
                            Session = session,
                            Stacktrace = exception.Stacktrace,
                        } by exception.ExceptionGroup into g
                        select new
                        {
                            ExceptionGroup = g.Key,
                            Occurrences = g.Count(),
                            Instances = g.OrderByDescending(e => e.Time).Take(maximumInstancesPerException),
                            AffectedUserCount = g.Select(e => e.Session.UserId).Distinct().Count()
                        } into row
                        where row.AffectedUserCount >= minimumNumberOfAffectedUsersForReportedExceptions
                        orderby row.ExceptionGroup.ExceptionGroupId
                        select row;

            int pos = 1;
            foreach (var result in query)
            {
                ws.Cells[pos, 1] = "C" + result.ExceptionGroup.ExceptionGroupId + ": " + result.ExceptionGroup.ExceptionType + " in " + result.ExceptionGroup.ExceptionLocation;
                Range range = ws.Cells[pos, 1];
                range.Font.Size = 21;
                range.Name = "Details_C" + result.ExceptionGroup.ExceptionGroupId;
                pos++;

                ws.Cells[pos, 1] = "Occurrences:";
                ws.Cells[pos++, 2] = result.Occurrences;
                ws.Cells[pos, 1] = "Affected users:";
                ws.Cells[pos++, 2] = result.AffectedUserCount;

                foreach (var instance in result.Instances)
                {
                    pos++; // empty row
                    ws.Cells[pos, 1] = "Date";
                    ws.Cells[pos++, 2] = instance.Time;
                    ws.Cells[pos, 1] = "User";
                    ws.Cells[pos++, 2] = instance.Session.UserId.ToString();
                    ws.Cells[pos, 1] = "Version";
                    ws.Cells[pos++, 2] = instance.Session.AppVersion;
                    ws.Cells[pos, 1] = "Stacktrace";
                    ws.Cells[pos, 2] = instance.Stacktrace;
                    ((Range)ws.Cells[pos, 2]).Font.Size = 9;
                    pos++;
                }
                pos++; // two empty rows
                pos++;
                Console.Write('.');
            }
            Console.WriteLine();
        }
        #endregion

        #region ExceptionList
        static void CreateExceptionList(ExcelWorkbook workbook, ReportRepository repository)
        {
            Worksheet ws = workbook.CreateSheet("Exception List");

            ws.Cells[1, 1] = "ID";
            ws.Columns[1].ColumnWidth = 7.5;
            ws.Cells[1, 2] = "Type";
            ws.Columns[2].ColumnWidth = 40;
            ws.Cells[1, 3] = "Location";
            ws.Columns[3].ColumnWidth = 60;
            ws.Cells[1, 4] = "Users affected";
            ws.Columns[4].ColumnWidth = 13;
            ws.Cells[1, 5] = "Occurrences";
            ws.Columns[5].ColumnWidth = 11;
            ws.Cells[1, 6] = "First Seen";
            ws.Columns[6].ColumnWidth = 9;
            ws.Cells[1, 7] = "Last Seen";
            ws.Columns[7].ColumnWidth = 9;
            ws.Cells[1, 8] = "Fixed In";
            ws.Columns[8].ColumnWidth = 5;

            const int commentCol = 9;
            ws.Columns[commentCol].ColumnWidth = 30;

            var query = from session in repository.Sessions
                        from exception in session.Exceptions
                        where exception.IsFirstInSession
                        group new
                        {
                            Time = exception.ThrownAt,
                            Session = session
                        } by exception.ExceptionGroup into g
                        select new
                        {
                            ExceptionGroup = g.Key,
                            Occurrences = g.Count(),
                            AffectedUserCount = g.Select(e => e.Session.UserId).Distinct().Count(),
                            FirstSeen = g.Min(e => e.Session.AppVersionRevision),
                            LastSeen = g.Max(e => e.Session.AppVersionRevision)
                        } into row
                        orderby row.AffectedUserCount descending, row.Occurrences descending
                        select row;

            int pos = 1;
            int excludedCount = 0;
            foreach (var row in query)
            {
                if (row.AffectedUserCount < minimumNumberOfAffectedUsersForReportedExceptions)
                {
                    excludedCount++;
                    continue;
                }

                pos++;
                ws.Cells[pos, 1] = "C" + row.ExceptionGroup.ExceptionGroupId;
                ws.Cells[pos, 2] = row.ExceptionGroup.ExceptionType;
                ws.Cells[pos, 3] = row.ExceptionGroup.ExceptionLocation;
                ws.Cells[pos, 4] = row.AffectedUserCount;
                ws.Cells[pos, 5] = row.Occurrences;
                ws.Cells[pos, 6] = row.FirstSeen;
                ws.Cells[pos, 7] = row.LastSeen;

                if (row.ExceptionGroup.UserFixedInRevision != null)
                {
                    int r = (int)row.ExceptionGroup.UserFixedInRevision;
                    ws.Cells[pos, 8] = r;

                    if (row.LastSeen >= r)
                        ws.Rows[pos].Style = "Bad";
                    else
                        ws.Rows[pos].Style = "Good";
                }
                if (!string.IsNullOrEmpty(row.ExceptionGroup.UserComment)) {
                    ws.Cells[pos, commentCol] = row.ExceptionGroup.UserComment;
                }

                ws.Hyperlinks.Add(ws.Cells[pos, 1], "", "Details_C" + row.ExceptionGroup.ExceptionGroupId);
                Console.Write('.');
            }

            ws.Columns[commentCol].Font.Color = System.Drawing.ColorTranslator.ToOle(System.Drawing.Color.Gray);
            ws.Columns[commentCol].Font.Italic = true;

            if (excludedCount > 0)
            {
                ws.Cells[++pos, 1] = excludedCount + " exceptions affecting less than " + minimumNumberOfAffectedUsersForReportedExceptions + " users were excluded from the report.";
            }
            Console.WriteLine();
        }
        #endregion
    }
}
