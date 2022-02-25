using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReliefLib
{
	public class ReliefFConsistent : ReliefF
	{
		public ReliefFConsistent(int k) : base(k)
		{ }

		protected override int GetProcessedIndex(Random r, int i)
		{
			return i;
		}
	}
}
