using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ReliefWeb.Models
{
	public class ReliefResult
	{
		public List<KeyValuePair<string, double>> Scores { get; set; }
		public KeyValuePair<string, double> BestScore { get; set; }
		public TimeSpan Duration { get; set; }
		public int? RemovedCount { get; set; }
	}
}