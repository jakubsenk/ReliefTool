using Aglomera.Evaluation.Internal;
using Aglomera;
using ReliefLib.Aglomera;
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
		public static List<double> GetIndex(ClusterTable clusters)
		{
			List<double> result = new List<double>();
			foreach (ClusterItem item in clusters.Rows)
			{
				DataUnit clusterItem = item.ClusterData.First();
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

				result.Add(1 - (average / minBetween));
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
