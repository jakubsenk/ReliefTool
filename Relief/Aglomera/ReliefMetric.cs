using Aglomera;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReliefLib.Aglomera
{
	internal class ReliefMetric : IDissimilarityMetric<AglomeraDataUnit>
	{
		public double Calculate(AglomeraDataUnit instance1, AglomeraDataUnit instance2)
		{
			double currentDistance = 0;
			for (int i = 0; i < instance1.CalcIndexes.Count; i++)
			{
				double feature1 = instance1.Columns[instance1.CalcIndexes[i]].NumericValue;
				double feature2 = instance2.Columns[instance1.CalcIndexes[i]].NumericValue;

				currentDistance += feature1 - feature2;
			}
			return currentDistance;
		}
	}
}
