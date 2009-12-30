using System;
using System.Collections.Specialized;
using System.Collections;

namespace SvnPostCommitHook
{
	/// <summary>
	/// Summary description for SvnLookOutputParser.
	/// </summary>
	public class SvnLookOutputParser
	{
		private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

		private string _author = "";
		private DateTime _timestamp = DateTime.MinValue;
		private string _message = "";
		private StringCollection _added = new StringCollection();
		private StringCollection _modified = new StringCollection();
		private StringCollection _deleted = new StringCollection();
		private StringCollection _propertyChanges = new StringCollection();
		private ArrayList _diffLines = new ArrayList();

		private SvnLookOutputParser(StringCollection infoLines, StringCollection changedLines, StringCollection diffLines)
		{
			StringEnumerator strings = infoLines.GetEnumerator();
			strings.MoveNext();  // move to first
			strings.MoveNext();  // skip first
			FindAuthor(strings);
			FindTimestamp(strings);
			FindLogMessageSize(strings);
			FindLogMessage(strings);

			strings = changedLines.GetEnumerator();
			bool hasMoreLines = SkipBlanks(strings);
			if (!hasMoreLines) 
				throw new ArgumentException("Unexpected: no changes recorded, aborting fatally");

			FindChanges(strings);

			if(diffLines != null && diffLines.Count > 0) 
			{
				strings = diffLines.GetEnumerator();
				hasMoreLines = SkipBlanks(strings);

				if (hasMoreLines)
					FillDiffCollection(strings);
			}
		}

		public static SvnLookOutputParser Parse(StringCollection infoLines, StringCollection changedLines) 
		{
			return Parse(infoLines, changedLines, null);
		}

		public static SvnLookOutputParser Parse(StringCollection infoLines, StringCollection changedLines, StringCollection diffLines) 
		{
			return new SvnLookOutputParser(infoLines, changedLines, diffLines);
		}

		public string Author 
		{
			get { return _author; }
		}

		public DateTime Timestamp 
		{
			get { return _timestamp; }
		}

		public string Message 
		{
			get { return _message; }
		}

		public StringCollection Added 
		{
			get { return _added; } // this should so be a copy
		}

		public StringCollection Modified 
		{
			get { return _modified; } // so should this
		}

		public StringCollection Deleted 
		{
			get { return _deleted; } // so should this
		}

		public ICollection DiffLines 
		{
			get { return _diffLines; }
		}

		private void FindAuthor(StringEnumerator strings) 
		{
			bool hasMoreLines = SkipBlanks(strings);
			if (!hasMoreLines) return;
				
			_author = strings.Current;
		}

		private void FindTimestamp(StringEnumerator strings) 
		{
			bool hasMoreLines = SkipBlanks(strings);
			if (!hasMoreLines) return;

			string[] parts = strings.Current.Split();
			_timestamp = DateTime.ParseExact(parts[0], "yyyy-MM-dd", System.Globalization.CultureInfo.InvariantCulture);
			_timestamp = _timestamp.Add(TimeSpan.Parse(parts[1]));
		}

		private int FindLogMessageSize(StringEnumerator strings) 
		{
			bool hasMoreLines = SkipBlanks(strings);
			if (!hasMoreLines) return 0;

			int size = int.Parse(strings.Current);
			return size;
		}

		private void FindLogMessage(StringEnumerator strings) 
		{
			bool hasMoreLines = SkipBlanks(strings);
			if (!hasMoreLines) return;

			System.Text.StringBuilder buffer = new System.Text.StringBuilder();
			do 
			{
				buffer.Append(strings.Current);
				buffer.Append(Environment.NewLine);
			} while(strings.MoveNext());

			_message = buffer.ToString().TrimEnd();
		}

		private void FindChanges(StringEnumerator changes) 
		{
			do 
			{
				string line = changes.Current.Trim();
				if(line == "") continue;
				int separatorPos = line.IndexOf(' ');
				string changeType = line.Substring(0, separatorPos);
				string path = line.Substring(separatorPos).TrimStart();

				switch(changeType) 
				{
					case "A":
						_added.Add(path);
						break;
					case "U":
						_modified.Add(path);
						break;
					case "D":
						_deleted.Add(path);
						break;
					case "_U":
					case "UU":
						_propertyChanges.Add(path);
						break;
					default:
						log.Warn("Unable to parse: " + line);
						break;
				}

			} while(changes.MoveNext());
		}

		private void FillDiffCollection(StringEnumerator strings) 
		{
			do 
			{
				DiffLine dl = DiffLine.Parse(strings.Current);
				_diffLines.Add(dl);
			} while(strings.MoveNext());
		}

		// bool indicates if StringEnumerator hasMoreLines? to parse
		private bool SkipBlanks(StringEnumerator strings) 
		{
			while(strings.MoveNext()) 
			{
				if(strings.Current != "") return true;
			}

			return false;
		}
	}

	public class DiffLine 
	{
		private string _line;
		private DiffLineType _lineType;

		private DiffLine(string line, DiffLineType lineType) 
		{
			_line = line;
			_lineType = lineType;
		}

		public string Line { get { return _line; } }
		public DiffLineType Type { get { return _lineType; } }

		public static DiffLine Parse(string line) 
		{
			if(line.Length > 0) 
			{
				switch(line[0]) 
				{
					case '+':
						if(line.Length > 1 && line[1] == '+') 
						{
							return new DiffLine(line, DiffLineType.NewRevisionFile);
						} 
						else 
						{
							return new DiffLine(line, DiffLineType.Addition);
						}
					case '-':
						if(line.Length > 1 && line[1] == '-') 
						{
							return new DiffLine(line, DiffLineType.OldRevisionFile);
						} 
						else 
						{
							return new DiffLine(line, DiffLineType.Deletion);
						}
					case '@':
						return new DiffLine(line, DiffLineType.ChangeScope);
					case '=':
						return new DiffLine(line, DiffLineType.Separator);
					case '\\':
						return new DiffLine(line, DiffLineType.Comment);
					case 'M':
					case 'A':
						return new DiffLine(line, DiffLineType.Filename);
					case ' ':
						return new DiffLine(line, DiffLineType.Unchanged);
					default:
						return new DiffLine(line, DiffLineType.Unknown);

				}
			} 
			else 
			{
				return new DiffLine(" ", DiffLineType.Blank);
			}
		}
	}

	public enum DiffLineType 
	{
		Addition,
		Deletion,
		Unchanged,
		Comment,
		Filename,
		Separator,
		OldRevisionFile,
		NewRevisionFile,
		ChangeScope,
		Unknown,
		Blank
		
	}
}
