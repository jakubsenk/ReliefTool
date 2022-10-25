using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReliefWeb.Models
{
	public class GraphModel
	{
		public List<List<double>> ClusterPairsX { get; set; } = new List<List<double>>();
		public List<List<double>> ClusterPairsY { get; set; } = new List<List<double>>();
		public List<Color> ClusterColors { get; set; } = new List<Color>();
		public string XAxis { get; set; }
		public string YAxis { get; set; }
	}
}
