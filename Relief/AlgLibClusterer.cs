using Aglomera.Linkage;
using Aglomera;
using ReliefLib.Aglomera;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ReliefLib.MyClusterer;

namespace ReliefLib
{
	public class AlgLibClusterer : IClusterer
	{
		public ClusterTable GetClusters(List<DataUnit> data, int clusterCount, List<int> clusterProperties)
		{
			alglib.clusterizerstate s;
			alglib.ahcreport rep;
			double[,] xy = new double[data.Count, clusterProperties.Count];
			int[] cidx;
			int[] cz;

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
			alglib.clusterizerrunahc(s, out rep);

			// with K=3 we have three clusters C0=[P3], C1=[P2,P4], C2=[P0,P1]
			alglib.clusterizergetkclusters(rep, clusterCount, out cidx, out cz);

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