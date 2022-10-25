using ReliefLib.MyClusterer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReliefLib
{
	public interface IClusterer
	{
		ClusterTable GetClusters(List<DataUnit> data, int clusterCount, List<int> clusterProperties = null);
	}
}
