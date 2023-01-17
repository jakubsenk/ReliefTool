using ReliefLib.MyClusterer;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Web;

namespace ReliefWeb.Models
{
	public class ReliefResult
	{
		public List<KeyValuePair<string, double>> Scores { get; set; }
		public List<KeyValuePair<string, double>> SortedScores { get; set; }
		public KeyValuePair<string, double> BestScore { get; set; }
		public TimeSpan Duration { get; set; }
		public int? RemovedCount { get; set; }
		public string Name { get; set; }
		public ClusterTable Clusters { get; set; }
		public ClusterTable ClustersAll { get; set; }
		public SillhouteModel Sillhoutte { get; set; }
		public SillhouteModel SillhoutteAll { get; set; }
		public List<GraphModel> Graphs { get; set; }
		public List<GraphModel> GraphsAll { get; set; }
	}
}