using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ReliefLib.MyClusterer;

namespace ReliefLib.AlgLib
{
	public class AlgLibKmeansClusterer : IClusterer
	{
		public ClusterTable GetClusters(List<DataUnit> data, int clusterCount, List<int> clusterProperties)
		{
			alglib.clusterizerstate s;
			alglib.kmeansreport rep;
			double[,] xy = new double[data.Count, clusterProperties.Count];
			int[] cidx;

			for (int j = 0; j < data.Count; j++)
			{
				DataUnit item = data[j];
				for (int i = 0; i < clusterProperties.Count; i++)
				{
					xy[j,i] = item.Columns[clusterProperties[i]].NumericValue;
				}
			}

			alglib.clusterizercreate(out s);
			alglib.clusterizersetpoints(s, xy, 2);
			alglib.clusterizerrunkmeans(s, clusterCount, out rep);

			if (rep.terminationtype != 1)
			{
				throw new ApplicationException("Clustering failed, result code: " + rep.terminationtype);
			}

			cidx = rep.cidx;

			ClusterTable result = new ClusterTable();
			for (int i = 0; i < clusterCount; i++)
			{
				ClusterItem row = new ClusterItem();
				row.ClusterData = new List<DataUnit>();
				result.Rows.Add(row);
			}


			for (int i = 0; i < cidx.Length; i++)
			{
				ClusterItem currentCluster = result.Rows[cidx[i]];
				currentCluster.ClusterData.Add(data[i]);
			}

			return result;
		}
	}
}