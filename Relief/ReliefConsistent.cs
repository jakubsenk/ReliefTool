using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReliefLib
{
	public class ReliefConsistent : Relief
	{
		protected override int GetProcessedIndex(Random r, int i)
		{
			return i;
		}
	}
}
