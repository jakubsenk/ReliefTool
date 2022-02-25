using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReliefLib
{
	public interface IReliefAlgorithm
	{
		List<double> Scores { get; }
		TimeSpan Elapsed { get; }

		void ProccessData(List<DataUnit> data);
	}
}
