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
		private bool debug;
		private List<DataUnit> data;
		private double[] weights;
		private Dictionary<string, int> stringValues;

		public Relief(List<DataUnit> data, bool debug = false)
		{
			this.debug = debug;
			this.data = data;
			ReliefInit();
		}

		private void ReliefInit()
		{
			stringValues = new Dictionary<string, int>();
			for (int i = 0; i < data[0].Columns.Count; i++)
			{
				int currentStringValue = 10;
				foreach (DataUnit item in data)
				{
					if (item.Columns[i].GetType() == typeof(string))
					{
						if (!stringValues.ContainsKey((string)item.Columns[i]))
						{
							stringValues.Add((string)item.Columns[i], currentStringValue);
							currentStringValue += 10;
						}
					}
				}
			}

			SetFeatureWeights();

			if (debug)
			{
				for (int i = 0; i < weights.Length; i++)
				{
					Console.WriteLine("W(" + (i + 1) + "): " + weights[i]);
				}
			}
		}

		private DataUnit GetSample(int sampleIndex)
		{
			if (sampleIndex >= data.Count || sampleIndex < 0)
			{
				if (debug)
				{
					Console.WriteLine("getSampleFeatures() index out of range");
				}
				return null;
			}
			else
			{
				return data[sampleIndex];
			}
		}

		private object GetSampleColumn(int sampleIndex, int featureIndex)
		{
			if (sampleIndex >= data.Count || sampleIndex < 0)
			{
				if (debug)
				{
					Console.WriteLine("getSampleFeature() index out of range");
				}
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
				if (debug)
				{
					Console.WriteLine("getSampleFeature() index out of range");
				}
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
					object feature1 = GetSampleColumn(sampleIndex, featureIndex);
					object feature2 = GetSampleColumn(i, featureIndex);
					if (feature1 == null || feature2 == null) continue;
					if (!feature1.GetType().IsEquivalentTo(feature2.GetType())) continue;

					if (feature1.GetType() == typeof(double))
					{
						currentDistance = (double)feature1 - (double)feature2;
					}
					else
					{
						currentDistance = stringValues[(string)feature1] - stringValues[(string)feature2];
					}
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
					object feature1 = GetSampleColumn(sampleIndex, featureIndex);
					object feature2 = GetSampleColumn(i, featureIndex);
					if (feature1 == null || feature2 == null) continue;
					if (!feature1.GetType().IsEquivalentTo(feature2.GetType())) continue;

					if (feature1.GetType() == typeof(double))
					{
						currentDistance = (double)feature1 - (double)feature2;
					}
					else
					{
						currentDistance = stringValues[(string)feature1] - stringValues[(string)feature2];
					}
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
			weights = new double[GetSample(0).Columns.Count];
			for (int i = 0; i < data.Count; i++)
			{
				for (int j = 0; j < weights.Length; j++)
				{
					weights[j] = weights[j] - Math.Pow(GetNearestHit(i, j), 2) + Math.Pow(GetNearestMiss(i, j), 2);
				}
			}
			for (int i = 0; i < weights.Length; i++)
			{
				weights[i] = weights[i] / data.Count;
			}
		}
	}
}
