using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

// https://www.researchgate.net/figure/Pseudo-code-of-the-ReliefF-algorithm_fig2_329963285
namespace ReliefLib
{
	public class ReliefF : IReliefAlgorithm
	{
		private int k;
		private List<DataUnit> data;
		private Stopwatch sw = new Stopwatch();

		public List<double> Scores { get; private set; }
		public TimeSpan Elapsed => sw.Elapsed;

		private Dictionary<string, double> classesProbabilities = new Dictionary<string, double>();
		private Dictionary<int, double> maxes = new Dictionary<int, double>();
		private Dictionary<int, double> mins = new Dictionary<int, double>();

		public ReliefF(int k)
		{
			this.k = k;
		}

		public void ProccessData(List<DataUnit> data)
		{
			sw.Start();
			this.data = data;
			foreach (DataUnit dataUnit in data)
			{
				if (!classesProbabilities.ContainsKey(dataUnit.ResultClass))
				{
					double prob = data.Where(x => x.ResultClass == dataUnit.ResultClass).Count() / (double)data.Count;
					classesProbabilities.Add(dataUnit.ResultClass, prob);
				}
			}
			for (int i = 0; i < data[0].Columns.Count; i++)
			{
				if (data[0].Columns[i].IsString) continue;
				maxes.Add(i, data.Max(x => x.Columns[i].NumericValue));
				mins.Add(i, data.Min(x => x.Columns[i].NumericValue));
			}
			SetFeatureWeights();
			sw.Stop();
		}

		private Dictionary<string, List<DataUnit>> GetNearestMisses(int index)
		{
			double currentDistance;
			Dictionary<int, double> distances = new Dictionary<int, double>();
			Dictionary<string, List<DataUnit>> result = new Dictionary<string, List<DataUnit>>();
			for (int i = 0; i < data.Count; i++)
			{
				if (data[i].ResultClass != data[index].ResultClass)
				{
					currentDistance = 0;
					for (int j = 0; j < data[index].Columns.Count; j++)
					{
						double feature1 = data[index].Columns[j].NumericValue;
						double feature2 = data[i].Columns[j].NumericValue;

						currentDistance += feature1 - feature2;
					}
					distances.Add(i, Math.Abs(currentDistance));
				}
			}
			distances = distances.OrderBy((x) => x.Value).ToDictionary(x => x.Key, x => x.Value);
			List<DataUnit> hits = new List<DataUnit>();
			foreach (string resClass in classesProbabilities.Keys)
			{
				result.Add(resClass, new List<DataUnit>());
			}
			foreach (KeyValuePair<int, double> item in distances)
			{
				DataUnit current = data[item.Key];
				if (result[current.ResultClass].Count < k)
					result[current.ResultClass].Add(current);
			}
			return result;
		}

		private List<DataUnit> GetNearestHits(int index)
		{
			double currentDistance;
			Dictionary<int, double> distances = new Dictionary<int, double>();
			for (int i = 0; i < data.Count; i++)
			{
				if (data[index].ResultClass == data[i].ResultClass && index != i)
				{
					currentDistance = 0;
					for (int j = 0; j < data[index].Columns.Count; j++)
					{
						double feature1 = data[index].Columns[j].NumericValue;
						double feature2 = data[i].Columns[j].NumericValue;

						currentDistance += feature1 - feature2;
					}
					distances.Add(i, Math.Abs(currentDistance));
				}
			}
			distances = distances.OrderBy((x) => x.Value).ToDictionary(x => x.Key, x => x.Value);
			List<DataUnit> hits = new List<DataUnit>();
			foreach (KeyValuePair<int, double> item in distances)
			{
				hits.Add(data[item.Key]);
				if (hits.Count == k) break;
			}
			return hits;
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
				List<DataUnit> hits = GetNearestHits(index);
				Dictionary<string, List<DataUnit>> misses = GetNearestMisses(index);

				for (int j = 0; j < Scores.Count; j++)
				{
					double hitsDiff = 0;
					double finalMissDiff = 0;
					foreach (var item in hits)
					{
						hitsDiff += Diff(j, ri, item) / (data.Count * k);
					}
					foreach (string resClass in classesProbabilities.Keys)
					{
						if (resClass != ri.ResultClass)
						{
							double prob = classesProbabilities[resClass] / (1 - classesProbabilities[ri.ResultClass]);
							double missDiff = 0;
							foreach (DataUnit item in misses[resClass])
							{
								missDiff += Diff(j, ri, item) / (data.Count * k);
							}
							finalMissDiff += prob * missDiff;
						}
					}
					Scores[j] += -hitsDiff + finalMissDiff;
				}

			}
		}

		private double Diff(int featureIndex, DataUnit a, DataUnit b)
		{
			if (!a.Columns[featureIndex].IsString && !b.Columns[featureIndex].IsString)
				return Math.Abs(a.Columns[featureIndex].NumericValue - b.Columns[featureIndex].NumericValue) / maxes[featureIndex] - mins[featureIndex];
			else return a.Columns[featureIndex].StringValue == b.Columns[featureIndex].StringValue ? 0 : 1;
		}
	}
}
