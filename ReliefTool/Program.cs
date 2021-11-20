using ReliefLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReliefTool
{
	class Program
	{
		static void Main(string[] args)
		{
			Relief r = new Relief(@"D:\Git\ReliefTool\ReliefTool\iris2.csv", true);
			Console.ReadKey();
		}
	}
}
