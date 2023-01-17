using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReliefWeb.Models
{
	public class SillhouteModel
	{
		public List<List<double>> ClusterPairs { get; set; } = new List<List<double>>();
		public List<Color> ClusterColors { get; set; } = new List<Color>();
	}
}
