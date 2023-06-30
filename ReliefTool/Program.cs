using ReliefLib;
using System;
using System.Collections.Generic;
using System.Diagnostics;
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
			PreparatorResult result = DataPreparator.PrepareData(File.ReadAllLines(args[0]));
			List<DataUnit> data = result.Data;

			Stopwatch sw = new Stopwatch();
			sw.Start();

			new ReliefF(3).ProccessData(data);
			Console.WriteLine("Calculation time ReliefF: " + sw.Elapsed);

			sw.Restart();
			new Relief().ProccessData(data);
			Console.WriteLine("Calculation time Relief: " + sw.Elapsed);

			Console.ReadKey();
		}
	}
}
