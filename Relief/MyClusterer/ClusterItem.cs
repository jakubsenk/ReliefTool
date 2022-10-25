using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReliefLib.MyClusterer
{
	public class ClusterItem
	{
		public List<double> Distances { get; set; }
		public List<DataUnit> ClusterData { get; set; }
	}
}
