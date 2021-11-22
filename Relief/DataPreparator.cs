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
				DataUnit row = new DataUnit(currentRow[currentRow.Length - 1]);
				for (int j = 0; j < currentRow.Length - 1; j++)
				{
					if (double.TryParse(currentRow[j], NumberStyles.Any, CultureInfo.InvariantCulture, out double d))
						row.Columns.Add(d);
					else
						row.Columns.Add(currentRow[j]);
				}
				result.Add(row);
			}

			return result;
		}
	}
}
