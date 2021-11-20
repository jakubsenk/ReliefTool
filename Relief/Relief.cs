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
		string[] fileContents;
		private List<int[]> samples;
		private double[] weights;

		public Relief(string file, bool debug = false)
		{
			this.debug = debug;
			ReliefInit(file);
		}

		private void ReliefInit(string file)
		{
			fileContents = File.ReadAllLines(file);
			fileContents = trimArray(fileContents);

			if (debug)
			{
				Console.WriteLine("fileContents.length after trim == " + fileContents.Length);
			}

			string[] currentRow;
			samples = new List<int[]>(fileContents.Length);
			for (int i = 0; i < fileContents.Length; i++)
			{
				currentRow = fileContents[i].Split(',');
				int[] row = new int[currentRow.Length];
				for (int j = 0; j < currentRow.Length; j++)
				{
					row[j] = int.Parse(currentRow[j]);
				}
				samples.Add(row);
			}

			setFeatureWeights();

			for (int i = 0; i < weights.Length; i++)
			{
				if (debug)
				{
					Console.WriteLine("W(" + (i + 1) + "): " + weights[i]);
				}
			}
		}

		private string[] trimArray(string[] _array)
		{

			int newLength = 0;
			for (int i = _array.Length - 1; (_array[i] == null) && (i >= 0); i--)
			{
				newLength = i;
			}
			if (newLength != 0)
			{
				string[] newArray = new string[newLength];
				Array.Copy(_array, 0, newArray, 0, newLength);
				return newArray;
			}
			else
			{
				return _array;
			}
		}

		private int[] getSampleFeatures(int sampleIndex)
		{
			if (sampleIndex >= samples.Count || sampleIndex < 0)
			{
				if (debug)
				{
					Console.WriteLine("getSampleFeatures() index out of range");
				}
				return null;
			}
			else
			{
				int[] featuresToReturn = new int[samples[sampleIndex].Length - 1];
				Array.Copy(samples[sampleIndex], 0, featuresToReturn, 0, featuresToReturn.Length);
				return featuresToReturn;
			}
		}

		private int getSampleFeature(int sampleIndex, int featureIndex)
		{
			if (sampleIndex >= samples.Count || sampleIndex < 0)
			{
				if (debug)
				{
					Console.WriteLine("getSampleFeature() index out of range");
				}
				throw new IndexOutOfRangeException();
			}
			else
			{
				return getSampleFeatures(sampleIndex)[featureIndex];
			}
		}

		private int getSampleClass(int sampleIndex)
		{
			if (sampleIndex >= samples.Count || sampleIndex < 0)
			{
				if (debug)
				{
					Console.WriteLine("getSampleFeature() index out of range");
				}
				throw new IndexOutOfRangeException();
			}
			else
			{
				return samples[sampleIndex][samples[sampleIndex].Length - 1];
			}
		}

		private int nearestMiss(int sampleIndex, int featureIndex)
		{
			int shortestDistance = -1;
			int currentDistance;
			for (int i = 0; i < samples.Count; i++)
			{
				if (getSampleClass(sampleIndex) != getSampleClass(i))
				{
					currentDistance = getSampleFeature(sampleIndex, featureIndex) - getSampleFeature(i, featureIndex);
					if (Math.Pow(currentDistance, 2) < Math.Pow(shortestDistance, 2) || shortestDistance == -1)
					{
						shortestDistance = currentDistance;
					}
				}
			}
			return shortestDistance;
		}

		private double nearestHit(int sampleIndex, int featureIndex)
		{
			double shortestDistance = -1;
			double currentDistance;
			for (int i = 0; i < samples.Count; i++)
			{
				if (getSampleClass(sampleIndex) == getSampleClass(i) && sampleIndex != i)
				{
					currentDistance = getSampleFeature(sampleIndex, featureIndex) - getSampleFeature(i, featureIndex);
					if (Math.Pow(currentDistance, 2) < Math.Pow(shortestDistance, 2) || shortestDistance == -1)
					{
						shortestDistance = currentDistance;
					}
				}
			}
			return shortestDistance;
		}

		private void setFeatureWeights()
		{
			weights = new double[getSampleFeatures(0).Length];
			weights[0] = 0.0;
			for (int i = 0; i < samples.Count; i++)
			{
				for (int j = 0; j < weights.Length; j++)
				{

					weights[j] = weights[j] - Math.Pow(nearestHit(i, j), 2) + Math.Pow(nearestMiss(i, j), 2);
				}
			}
			for (int i = 0; i < weights.Length; i++)
			{
				weights[i] = weights[i] / samples.Count;
			}
		}
	}
}
