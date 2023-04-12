using ReliefLib;
using System.Data;
using System.Text;

namespace TextMapper
{
	internal class Program
	{
		static void Main(string[] args)
		{
			if (args.Length == 0)
			{
				Console.WriteLine("No input specified");
				return;
			}
			string[] content = File.ReadAllLines(args[0]).Skip(1).ToArray();

			List<string> list = new List<string>();
			for (int i = 0; i < content.Length; i++)
			{
				bool spam = content[i].StartsWith("spam");
				content[i] = content[i].Substring(spam ? 4 : 3).TrimStart(',').Trim('"');
				list.Add(spam ? "SPAM" : "HAM");
			}

			HashSet<string> reserverWords = new HashSet<string>
			{
				"a", "an", "and", "are", "as", "at", "be", "by", "for", "from", "has", "he", "in", "is",
				"it", "its", "of", "on", "that", "the", "to", "was", "were", "will", "with", "i"
			};

			DataTable table = new DataTable();
			PorterStemmer ps = new PorterStemmer();
			bool ham = true;
			for (int i = 0, j = 0; i < content.Length; i++)
			{
				if (ham && list[i] != "HAM")
				{
					continue;
				}
				else if (!ham && list[i] != "SPAM")
				{
					continue;
				}
				ham = !ham;
				string input = content[i].Replace('\n', ' ').Replace('\t', ' ').Replace('\r', ' ').Replace('(', ' ').Replace(')', ' ').Replace('[', ' ').Replace(']', ' ').Replace(';', ' ').Replace('<', ' ').Replace('>', ' ')
					.Replace("\"", "").Replace(',', ' ').Replace('.', ' ').Replace('—', ' ').Replace('!', ' ').Replace('?', ' ').Replace('*', ' ').Replace(':', ' ').Replace('“', ' ').Replace('”', ' ').ToLower();

				string[] words = input.Split(' ', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);

				table.Rows.Add();
				foreach (string word in words)
				{
					string stemmed = ps.StemWord(word);
					if (reserverWords.Contains(stemmed)) continue;
					if (table.Columns[stemmed] == null)
					{
						table.Columns.Add(new DataColumn(stemmed, typeof(int)) { AllowDBNull = false, DefaultValue = 0 });
					}
					int current = (int)table.Rows[j][stemmed];
					table.Rows[j][stemmed] = current + 1;
				}
				j++;
			}

			StringBuilder sb = new StringBuilder();
			for (int i = 0; i < table.Columns.Count; i++)
			{
				sb.Append(table.Columns[i].ColumnName).Append(',');
			}
			sb.AppendLine("Type");
			ham = true;
			for (int i = 0; i < table.Rows.Count; i++)
			{
				foreach (DataColumn col in table.Columns)
				{
					sb.Append(table.Rows[i][col]).Append(',');
				}
				sb.AppendLine(ham ? "HAM" : "SPAM");
				ham = !ham;
			}

			File.WriteAllText("output.csv", sb.ToString());
		}
	}
}