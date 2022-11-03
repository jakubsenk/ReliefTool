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
		public static PreparatorResult PrepareData(string[] content)
		{
			return PrepareData(content, new DataPreparatorOptions());
		}

		public static PreparatorResult PrepareData(string[] content, DataPreparatorOptions options)
		{
			string[] currentRow;
			HashSet<int> stringColumnIndexes = new HashSet<int>();

			List<DataUnit> result = new List<DataUnit>(content.Length);
			for (int i = options.SkipFirstLine ? 1 : 0; i < content.Length; i++)
			{
				currentRow = content[i].Split(options.ColumnSeparator);
				for (int j = 0; j < currentRow.Length; j++)
				{
					if (currentRow[j] == null) continue;
					if (currentRow[j].StartsWith("\"") && !currentRow[j].EndsWith("\""))
					{
						currentRow[j] = currentRow[j] + options.ColumnSeparator + currentRow[j + 1];
						currentRow[j + 1] = null;
						currentRow = currentRow.Where(x => x != null).ToArray();
						j--;
					}
				}

				DataUnit row = new DataUnit(currentRow[options.ResultClassIsFirstColumn ? 0 : currentRow.Length - 1]);
				for (int j = options.ResultClassIsFirstColumn ? 1 : 0; j < currentRow.Length - (options.ResultClassIsFirstColumn ? 0 : 1); j++)
				{
					string current = currentRow[j].Trim('"');
					ColumnValue value = new ColumnValue();
					value.IsString = false;
					if (string.IsNullOrEmpty(current))
						throw new Exception("Application does not support misisng values.");
					else if (double.TryParse(current.Replace(",", "."), NumberStyles.Any, CultureInfo.InvariantCulture, out double d))
					{
						value.NumericValue = d;
					}
					else
					{
						if (DateTime.TryParse(current, out DateTime date))
						{
							value.NumericValue = date.Ticks;
						}
						else if (bool.TryParse(current, out bool boolValue))
						{
							value.NumericValue = boolValue ? 1 : 0;
						}
						else
						{
							value.IsString = true;
							value.StringValue = current;
							if (!stringColumnIndexes.Contains(j))
								stringColumnIndexes.Add(j);
						}
					}
					row.Columns.Add(value);
				}
				result.Add(row);
			}

			int columnCount = result[0].Columns.Count;
			foreach (DataUnit item in result)
			{
				if (item.Columns.Count != columnCount)
					throw new Exception("Unable to parse data file. All rows must have same column count.");
			}

			List<string> keys = GetColumnDefinitions(content, options);

			foreach (int i in stringColumnIndexes)
			{
				foreach (DataUnit item in result)
				{
					if (!item.Columns[i].IsString)
					{
						item.Columns[i].StringValue = item.Columns[i].NumericValue.ToString();
						item.Columns[i].NumericValue = default;
						item.Columns[i].IsString = true;
					}
				}
			}

			columnCount = result[0].Columns.Count;

			Dictionary<DataUnit, DataUnit> original = new Dictionary<DataUnit, DataUnit>();

			if (options.Normalize)
			{
				for (int i = 0; i < result.Count; i++)
				{
					DataUnit unit = DataUnit.Clone(result[i]);
					original.Add(result[i], unit);
				}
				for (int i = 0; i < columnCount; i++)
				{
					if (result[0].Columns[i].IsString) continue;
					double max = result.Max(x => x.Columns[i].NumericValue);
					double min = result.Min(x => x.Columns[i].NumericValue);

					double range = max - min;
					if (range > 0)
					{
						foreach (DataUnit row in result)
						{
							row.Columns[i].NumericValue = (row.Columns[i].NumericValue - min) / range;
						}
					}
					else if (max > 1 || max < 0)
					{
						foreach (DataUnit row in result)
						{
							row.Columns[i].NumericValue = max;
						}
					}
				}
			}

			PreparatorResult presult = new PreparatorResult(result, keys, original);
			return presult;
		}

		private static List<string> GetColumnDefinitions(string[] content, DataPreparatorOptions options)
		{
			List<string> result = new List<string>();
			if (options.SkipFirstLine)
			{
				string[] firstRow = content[0].Split(options.ColumnSeparator);
				for (int j = 0; j < firstRow.Length - 1; j++)
				{
					result.Add(firstRow[j]);
				}
			}
			else
			{
				string[] firstRow = content[1].Split(options.ColumnSeparator);
				for (int i = 0; i < firstRow.Length - 1; i++)
				{
					result.Add(i.ToString());
				}
			}

			return result;
		}
	}
}
