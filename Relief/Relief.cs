using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

// https://github.com/ansballard/Relief-Algorithms/blob/master/Relief.java
namespace ReliefLib
{
	public class Relief
	{
		private bool parallel;
		private List<DataUnit> data;

		public List<double> Scores { get; private set; }

		public Relief(List<DataUnit> data, bool parallel = false)
		{
			this.parallel = parallel;
			this.data = data;
			ReliefInit();
		}

		private void ReliefInit()
		{
			SetFeatureWeights();
		}

		private DataUnit GetSample(int sampleIndex)
		{
			if (sampleIndex >= data.Count || sampleIndex < 0)
			{
				return null;
			}
			else
			{
				return data[sampleIndex];
			}
		}

		private double? GetSampleColumn(int sampleIndex, int featureIndex)
		{
			if (sampleIndex >= data.Count || sampleIndex < 0)
			{
				throw new IndexOutOfRangeException();
			}
			else
			{
				return GetSample(sampleIndex).Columns[featureIndex];
			}
		}

		private object GetSampleClass(int sampleIndex)
		{
			if (sampleIndex >= data.Count || sampleIndex < 0)
			{
				throw new IndexOutOfRangeException();
			}
			else
			{
				return data[sampleIndex].ResultClass;
			}
		}

		private double GetNearestMiss(int sampleIndex, int featureIndex)
		{
			double shortestDistance = -1;
			double currentDistance;
			for (int i = 0; i < data.Count; i++)
			{
				if (!string.Equals(GetSampleClass(sampleIndex), GetSampleClass(i)))
				{
					double? feature1 = GetSampleColumn(sampleIndex, featureIndex);
					double? feature2 = GetSampleColumn(i, featureIndex);
					if (feature1 == null || feature2 == null) continue;

					currentDistance = (double)feature1 - (double)feature2;

					if (Math.Pow(currentDistance, 2) < Math.Pow(shortestDistance, 2) || shortestDistance == -1)
					{
						shortestDistance = currentDistance;
					}
				}
			}
			return shortestDistance;
		}

		private double GetNearestHit(int sampleIndex, int featureIndex)
		{
			double shortestDistance = -1;
			double currentDistance;
			for (int i = 0; i < data.Count; i++)
			{
				if (string.Equals(GetSampleClass(sampleIndex), GetSampleClass(i)) && sampleIndex != i)
				{
					double? feature1 = GetSampleColumn(sampleIndex, featureIndex);
					double? feature2 = GetSampleColumn(i, featureIndex);
					if (feature1 == null || feature2 == null) continue;

					currentDistance = (double)feature1 - (double)feature2;

					if (Math.Pow(currentDistance, 2) < Math.Pow(shortestDistance, 2) || shortestDistance == -1)
					{
						shortestDistance = currentDistance;
					}
				}
			}
			return shortestDistance;
		}

		private void SetFeatureWeights()
		{
			int featureCount = GetSample(0).Columns.Count;
			Scores = new List<double>(GetSample(0).Columns.Count);
			for (int i = 0; i < featureCount; i++)
			{
				Scores.Add(0);
			}
			ParallelOptions ops = new ParallelOptions()
			{
				MaxDegreeOfParallelism = 4
			};
			for (int i = 0; i < data.Count; i++)
			{
				if (parallel)
				{
					Parallel.For(0, featureCount, ops, j =>
						{
							Scores[j] = Scores[j] - Math.Pow(GetNearestHit(i, j), 2) + Math.Pow(GetNearestMiss(i, j), 2);
						});
				}
				else
				{
					for (int j = 0; j < featureCount; j++)
					{
						Scores[j] = Scores[j] - Math.Pow(GetNearestHit(i, j), 2) + Math.Pow(GetNearestMiss(i, j), 2);
					}
				}
			}
			for (int i = 0; i < Scores.Count; i++)
			{
				Scores[i] = Scores[i] / data.Count;
			}
		}
	}
}
