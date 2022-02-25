using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

// https://github.com/ansballard/Relief-Algorithms/blob/master/Relief.java
namespace ReliefLib
{
	public class ReliefJava : IReliefAlgorithm
	{
		private List<DataUnit> data;
		private Stopwatch sw = new Stopwatch();

		public List<double> Scores { get; private set; }
		public TimeSpan Elapsed => sw.Elapsed;

		public void ProccessData(List<DataUnit> data)
		{
			sw.Start();
			this.data = data;
			SetFeatureWeights();
			sw.Stop();
		}

		private double GetNearestMiss(int sampleIndex, int featureIndex)
		{
			double shortestDistance = int.MaxValue;
			double currentDistance;
			for (int i = 0; i < data.Count; i++)
			{
				if (data[sampleIndex].ResultClass != data[i].ResultClass)
				{
					double feature1 = data[sampleIndex].Columns[featureIndex];
					double feature2 = data[i].Columns[featureIndex];

					currentDistance = feature1 - feature2;

					if (Math.Pow(currentDistance, 2) < Math.Pow(shortestDistance, 2))
					{
						shortestDistance = currentDistance;
					}
				}
			}
			return shortestDistance;
		}

		private double GetNearestHit(int sampleIndex, int featureIndex)
		{
			double shortestDistance = int.MaxValue;
			double currentDistance;
			for (int i = 0; i < data.Count; i++)
			{
				if (data[sampleIndex].ResultClass == data[i].ResultClass && sampleIndex != i)
				{
					double feature1 = data[sampleIndex].Columns[featureIndex];
					double feature2 = data[i].Columns[featureIndex];

					currentDistance = feature1 - feature2;

					if (Math.Pow(currentDistance, 2) < Math.Pow(shortestDistance, 2))
					{
						shortestDistance = currentDistance;
					}
				}
			}
			return shortestDistance;
		}

		private void SetFeatureWeights()
		{
			int featureCount = data[0].Columns.Count;
			Scores = new List<double>(data[0].Columns.Count);
			for (int i = 0; i < featureCount; i++)
			{
				Scores.Add(0);
			}
			for (int i = 0; i < data.Count; i++)
			{
				for (int j = 0; j < featureCount; j++)
				{
					Scores[j] = Scores[j] - Math.Pow(GetNearestHit(i, j), 2) + Math.Pow(GetNearestMiss(i, j), 2);
				}
			}
			for (int i = 0; i < Scores.Count; i++)
			{
				Scores[i] = Scores[i] / data.Count;
			}
		}
	}
}
