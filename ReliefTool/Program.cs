using ReliefLib;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReliefTool
{
	class Program
	{
		static void Main(string[] args)
		{
			DataPreparatorOptions options = new DataPreparatorOptions
			{
				SkipFirstLine = true,
				ColumnSeparator = ';'
			};
			List<DataUnit> data = DataPreparator.PrepareData(File.ReadAllLines(@"D:\Git\ReliefTool\ReliefTool\weather2.csv"), options);
			Relief r = new Relief(data, true);
			Console.ReadKey();
		}
	}
}
