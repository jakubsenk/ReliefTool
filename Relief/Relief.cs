using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReliefLib
{
	// https://link.springer.com/content/pdf/10.1023/A:1025667309714.pdf
	public class Relief : IReliefAlgorithm
	{
		private List<DataUnit> data;
		private Stopwatch sw = new Stopwatch();

		public List<double> Scores { get; private set; }
		public TimeSpan Elapsed => sw.Elapsed;

		private Dictionary<int, double> maxes = new Dictionary<int, double>();
		private Dictionary<int, double> mins = new Dictionary<int, double>();

		public void ProccessData(List<DataUnit> data)
		{
			sw.Start();
			this.data = data;
			for (int i = 0; i < data[0].Columns.Count; i++)
			{
				maxes.Add(i, data.Max(x => x.Columns[i]));
				mins.Add(i, data.Min(x => x.Columns[i]));
			}
			SetFeatureWeights();
			sw.Stop();
		}

		private DataUnit GetNearestMiss(int sampleIndex)
		{
			double shortestDistance = int.MaxValue;
			double currentDistance;
			int index = -1;
			for (int i = 0; i < data.Count; i++)
			{
				if (data[i].ResultClass != data[sampleIndex].ResultClass)
				{
					currentDistance = 0;
					for (int j = 0; j < data[sampleIndex].Columns.Count; j++)
					{
						double feature1 = data[sampleIndex].Columns[j];
						double feature2 = data[i].Columns[j];

						currentDistance += feature1 - feature2;
					}
					if (Math.Pow(currentDistance, 2) < Math.Pow(shortestDistance, 2))
					{
						shortestDistance = currentDistance;
						index = i;
					}
				}
			}

			return data[index];
		}

		private DataUnit GetNearestHit(int sampleIndex)
		{
			double shortestDistance = int.MaxValue;
			double currentDistance;
			int index = -1;
			for (int i = 0; i < data.Count; i++)
			{
				if (data[sampleIndex].ResultClass == data[i].ResultClass && sampleIndex != i)
				{
					currentDistance = 0;
					for (int j = 0; j < data[i].Columns.Count; j++)
					{
						double feature1 = data[sampleIndex].Columns[j];
						double feature2 = data[i].Columns[j];

						currentDistance += feature1 - feature2;
					}
					if (Math.Pow(currentDistance, 2) < Math.Pow(shortestDistance, 2))
					{
						shortestDistance = currentDistance;
						index = i;
					}
				}
			}
			return data[index];
		}

		protected virtual int GetProcessedIndex(Random r, int i)
		{
			return r.Next(data.Count);
		}

		private void SetFeatureWeights()
		{
			int featureCount = data[0].Columns.Count;
			Scores = new List<double>(data[0].Columns.Count);
			for (int i = 0; i < featureCount; i++)
			{
				Scores.Add(0);
			}
			Random r = new Random();
			for (int i = 0; i < data.Count; i++)
			{
				int index = GetProcessedIndex(r, i);
				DataUnit ri = data[index];
				DataUnit hit = GetNearestHit(index);
				DataUnit miss = GetNearestMiss(index);

				for (int j = 0; j < Scores.Count; j++)
				{
					double hitDiff = Diff(j, ri, hit);
					double missDiff = Diff(j, ri, miss);
					Scores[j] += -hitDiff + missDiff;
				}
			}
			for (int i = 0; i < Scores.Count; i++)
			{
				Scores[i] = Scores[i] / data.Count;
			}
		}

		private double Diff(int featureIndex, DataUnit a, DataUnit b)
		{
			return Math.Abs(a.Columns[featureIndex] - b.Columns[featureIndex]) / maxes[featureIndex] - mins[featureIndex];
		}
	}
}
