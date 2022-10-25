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
	public class InternalClusterer : IClusterer
	{
		public ClusterTable GetClusters(List<DataUnit> data, int clusterCount, List<int> clusterProperties)
		{
			AgglomerativeClustering alg = new AgglomerativeClustering(clusterProperties);
			return alg.GetClusters(data, clusterCount);
		}
	}
}