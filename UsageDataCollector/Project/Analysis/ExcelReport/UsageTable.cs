using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ExcelReport.DataAccess;
using System.Collections.ObjectModel;

namespace ExcelReport
{
    class UsageTable
    {
        DateTime start, end;
        List<Session> sessions;
        Version[] knownVersions;

        public UsageTable(IEnumerable<Session> sessions, Version[] knownVersions)
        {
            this.sessions = sessions.ToList();
            this.knownVersions = knownVersions;
            if (this.sessions.Count == 0)
            {
                start = DateTime.Now;
                end = DateTime.Now;
            }
            else
            {
                start = this.sessions.Min(s => s.StartTime);
                end = this.sessions.Max(s => s.StartTime);
            }
        }

        public class Row {
            public DateTime Date;
            public int[] Values;
        }

        public ReadOnlyCollection<Row> CreateDaily()
        {
            return Create(time => time.Date, d => d.AddDays(1));
        }

        public ReadOnlyCollection<Row> CreateWeekly()
        {
            return Create(time => time.Date.AddDays(-(int)time.DayOfWeek), d => d.AddDays(7));
        }

        public ReadOnlyCollection<Row> CreateMonthly()
        {
            return Create(time => new DateTime(time.Year, time.Month, 1), d => d.AddMonths(1));
        }

        ReadOnlyCollection<Row> Create(Func<DateTime, DateTime> roundToTimeUnit, Func<DateTime, DateTime> increment)
        {
            List<Row> rows = new List<Row>();
            DateTime date = roundToTimeUnit(start);
            var dictByDate = ToMultiDictionary(sessions.GroupBy(s => new { s.UserId, Date = roundToTimeUnit(s.StartTime) }), g => g.Key.Date);
            while (date <= end)
            {
                IEnumerable<IEnumerable<Session>> sessionsGroupedByUser;
                if (dictByDate.ContainsKey(date))
                    sessionsGroupedByUser = dictByDate[date];
                else
                    sessionsGroupedByUser = Enumerable.Empty<IEnumerable<Session>>();

                Row newRow = new Row();
                newRow.Date = date;
                newRow.Values = new int[knownVersions.Length + 1];

                int otherCount = sessionsGroupedByUser.Count();
                for (int i = 0; i < knownVersions.Length; i++)
                {
                    int c = sessionsGroupedByUser.Where(g => g.Max(s => s.AppVersion) == knownVersions[i]).Count();
                    newRow.Values[i] = c;
                    otherCount -= c;
                }
                newRow.Values[knownVersions.Length] = otherCount;

                rows.Add(newRow);
                date = increment(date);
            }
            return rows.AsReadOnly();
        }

        static Dictionary<TKey, IGrouping<TKey, TValue>> ToMultiDictionary<TKey, TValue>(IEnumerable<TValue> input, Func<TValue, TKey> keySelector)
        {
            return input.GroupBy(keySelector).ToDictionary(g => g.Key);
        }
    }
}
