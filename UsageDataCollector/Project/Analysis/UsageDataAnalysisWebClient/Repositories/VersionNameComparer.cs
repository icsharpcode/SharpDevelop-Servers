using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace UsageDataAnalysisWebClient.Repositories
{
	public class VersionNameComparer : IComparer<string>
	{
		public int Compare(string x, string y)
		{
			Token[] a = Tokenize(x).ToArray();
			Token[] b = Tokenize(y).ToArray();
			for (int i = 0; i < Math.Min(a.Length, b.Length); i++)
			{
				int r = a[i].CompareTo(b[i]);
				if (r != 0)
					return r;
			}
			return a.Length.CompareTo(b.Length);
		}

		IEnumerable<Token> Tokenize(string text)
		{
			int pos = 0;
			while (pos < text.Length) {
				if (text[pos] == '-') {
					yield return new Token(TypeDash, "-");
					pos++;
				} else if (char.IsDigit(text[pos])) {
					int startPos = pos;
					while (pos < text.Length && char.IsDigit(text[pos])) 
						pos++;
					yield return new Token(TypeNumber, text.Substring(startPos, pos - startPos));
				} else {
					int startPos = pos;
					while (pos < text.Length && !(text[pos] == '-' || char.IsDigit(text[pos])))
						pos++;
					yield return new Token(TypeText, text.Substring(startPos, pos - startPos));
				}
			}
		}

		const int TypeDash = 0;
		const int TypeNumber = 1;
		const int TypeText = 2;

		class Token : IComparable<Token>
		{
			int type;
			string text;
			ulong number;

			public Token(int type, string text)
			{
				this.type = type;
				switch (type) {
					case TypeText:
						this.text = text;
						break;
					case TypeNumber:
						this.number = ulong.Parse(text);
						break;
				}
			}

			public int CompareTo(Token other)
			{
				int r = type.CompareTo(other.type);
				if (r != 0)
					return r;
				// types are equal
				switch (type) {
					case TypeNumber:
						return number.CompareTo(other.number);
					case TypeText:
						return string.CompareOrdinal(text, other.text);
					default:
						return 0;
				}
			}
		}
	}
}