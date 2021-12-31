using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReliefLib
{
	public class DataPreparator
	{
		public static List<DataUnit> PrepareData(string[] content)
		{
			return PrepareData(content, new DataPreparatorOptions());
		}

		public static List<DataUnit> PrepareData(string[] content, DataPreparatorOptions options)
		{
			string[] currentRow;
			List<DataUnit> result = new List<DataUnit>(content.Length);
			for (int i = options.SkipFirstLine ? 1 : 0; i < content.Length; i++)
			{
				currentRow = content[i].Split(options.ColumnSeparator);
				DataUnit row = new DataUnit(currentRow[options.ResultClassIsFirstColumn ? 0 : currentRow.Length - 1]);
				for (int j = options.ResultClassIsFirstColumn ? 1 : 0; j < currentRow.Length - (options.ResultClassIsFirstColumn ? 0 : 1); j++)
				{
					if (string.IsNullOrEmpty(currentRow[j]))
						row.Columns.Add(null);
					else if (double.TryParse(currentRow[j].Replace(",", "."), NumberStyles.Any, CultureInfo.InvariantCulture, out double d))
						row.Columns.Add(d);
					else
						throw new Exception("All values must be numeric.");
				}
				result.Add(row);
			}

			int columnCount = result[0].Columns.Count;
			foreach (var item in result)
			{
				if (item.Columns.Count != columnCount)
					throw new Exception("Unable to parse data file. All rows must have same column count.");
			}

			return result;
		}

		public static List<string> GetColumnDefinitions(string[] content, DataPreparatorOptions options)
		{
			List<string> result = new List<string>();
			string[] firstRow = content[0].Split(options.ColumnSeparator);
			for (int j = 0; j < firstRow.Length - 1; j++)
			{
				result.Add(firstRow[j]);
			}

			return result;
		}
	}
}
