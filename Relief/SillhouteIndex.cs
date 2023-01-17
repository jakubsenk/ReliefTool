using ReliefLib.MyClusterer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReliefLib
{
	// https://stackoverflow.com/questions/23387275/how-do-you-manually-compute-for-silhouette-cohesion-and-separation-of-cluster
	public static class SillhouteIndex
	{
		public static List<List<double>> GetIndex(ClusterTable clusters)
		{
			List<List<double>> result = new List<List<double>>();
			foreach (ClusterItem item in clusters.Rows)
			{
				List<double> current = new List<double>();
				for (int i = 0; i < item.ClusterData.Count; i++)
				{
					DataUnit clusterItem = item.ClusterData[i];
					double average = item.ClusterData.Average(x => GetDistance(clusterItem, x));
					double minBetween = double.MaxValue;
					foreach (ClusterItem otherCluster in clusters.Rows)
					{
						if (item != otherCluster)
						{
							double averageBetween = otherCluster.ClusterData.Average(x => GetDistance(clusterItem, x));
							if (averageBetween < minBetween) minBetween = averageBetween;
						}
					}

					current.Add(1 - (average / minBetween));
				}
				result.Add(current.OrderByDescending(x => x).ToList());
			}

			return result;
		}

		private static double GetDistance(DataUnit a, DataUnit b)
		{
			double distance = 0;
			for (int i = 0; i < a.Columns.Count; i++)
			{
				distance += Math.Pow(a.Columns[i].NumericValue - b.Columns[i].NumericValue, 2);
			}
			return Math.Sqrt(distance);
		}
	}
}
