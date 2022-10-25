using Aglomera.Linkage;
using Aglomera;
using ReliefLib.Aglomera;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ReliefLib.MyClusterer;
using Aglomera.Evaluation.Internal;

namespace ReliefLib
{
	public class AglomeraClusterer : IClusterer
	{
		public ClusterTable GetClusters(List<DataUnit> data, int clusterCount, List<int> clusterProperties)
		{
			ReliefMetric metric = new ReliefMetric();
			SingleLinkage<AglomeraDataUnit> linkage = new SingleLinkage<AglomeraDataUnit>(metric);
			AgglomerativeClusteringAlgorithm<AglomeraDataUnit> algorithm = new AgglomerativeClusteringAlgorithm<AglomeraDataUnit>(linkage);

			HashSet<AglomeraDataUnit> agData = new HashSet<AglomeraDataUnit>();
			foreach (DataUnit item in data)
			{
				AglomeraDataUnit unit = new AglomeraDataUnit(clusterProperties);
				unit.Columns.AddRange(item.Columns);
				unit.ResultClass = item.ResultClass;
				agData.Add(unit);
			}
			ClusteringResult<AglomeraDataUnit> clusteringResult = algorithm.GetClustering(agData);

			ClusterSet<AglomeraDataUnit> cluster = clusteringResult[clusteringResult.Count - clusterCount];

			return new ClusterTable();
		}
	}
}