using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReliefLib.MyClusterer
{
	public class AgglomerativeClustering
	{
		private int parallelFrom;

		private List<int> clusterProps = new List<int>();

		public AgglomerativeClustering(List<int> clusterProperties, int parallelFrom = 500)
		{
			this.parallelFrom = parallelFrom;
			clusterProps.AddRange(clusterProperties);
		}
		public ClusterTable GetClusters(List<DataUnit> list, int clusters)
		{
			ClusterTable table = new ClusterTable();
			for (int i = 0; i < list.Count; i++)
			{
				ClusterItem row = new ClusterItem();
				row.Distances = new List<double>(list.Count);
				row.ClusterData = new List<DataUnit>() { list[i] };
				table.Rows.Add(row);
			}

			if (list.Count < parallelFrom)
				for (int i = 0; i < list.Count; i++)
				{
					for (int j = 0; j < list.Count; j++)
					{
						table.Rows[i].Distances.Add(GetEucledianDistance(list[i], list[j]));
					}
				}
			else
				Parallel.For(0, list.Count, (i) =>
				{
					for (int j = 0; j < list.Count; j++)
					{
						table.Rows[i].Distances.Add(GetEucledianDistance(list[i], list[j]));
					}
				});

			int count = clusters;

			while (table.Rows.Count > count)
			{
				ClusterItem minCol = null;
				int rowIndex = 0;
				int minColIndex = 0;
				double minDistance = 800;
				if (table.Rows.Count < parallelFrom)
				{
					for (int i = 0; i < table.Rows.Count; i++)
					{
						for (int j = 0; j < table.Rows.Count; j++)
						{
							if (i == j) continue;
							double distance = table.Rows[i].Distances[j];
							if (distance < minDistance)
							{
								minDistance = distance;
								minCol = table.Rows[j];
								minColIndex = j;
								rowIndex = i;
							}
						}
					}
				}
				else
				{
					Parallel.For(0, table.Rows.Count, (i) =>
					{
						for (int j = 0; j < table.Rows.Count; j++)
						{
							if (i == j) continue;
							double distance = table.Rows[i].Distances[j];
							if (distance < minDistance)
							{
								lock (this)
								{
									if (distance < minDistance)
									{
										minDistance = distance;
										minCol = table.Rows[j];
										minColIndex = j;
										rowIndex = i;
									}
								}
							}
						}
					});
				}

				table.Rows[rowIndex].ClusterData.AddRange(minCol.ClusterData);
				if (minColIndex < rowIndex) rowIndex--;
				table.Rows.Remove(minCol);

				if (table.Rows.Count < parallelFrom)
					for (int i = 0; i < table.Rows.Count; i++)
					{
						table.Rows[i].Distances.RemoveAt(minColIndex);
					}
				else
					Parallel.For(0, table.Rows.Count, i =>
					{
						table.Rows[i].Distances.RemoveAt(minColIndex);
					});

				table.Rows[rowIndex].Distances.Clear();
				for (int i = 0; i < table.Rows.Count; i++)
				{
					double d = GetSingleLinkDistance(table.Rows[rowIndex].ClusterData, table.Rows[i].ClusterData);
					table.Rows[rowIndex].Distances.Add(d);
				}

				//printDebug(table);
			}
			return table;
		}

		private double GetEucledianNorm(DataUnit value)
		{
			double sum = 0;
			foreach (ColumnValue item in value.Columns)
			{
					sum += Math.Pow(item.NumericValue, 2);
			}

			return Math.Sqrt(sum);
		}

		private double GetEucledianDistance(DataUnit a, DataUnit b)
		{
			DataUnit subtracted = new DataUnit();
			subtracted.Columns = new List<ColumnValue>(clusterProps.Count);
			for (int i = 0; i < clusterProps.Count; i++)
			{
				ColumnValue cv = new ColumnValue();
				cv.NumericValue = a.Columns[clusterProps[i]].NumericValue - b.Columns[clusterProps[i]].NumericValue;
				subtracted.Columns.Add(cv);
			}
			return GetEucledianNorm(subtracted);
		}

		private double GetSingleLinkDistance(List<DataUnit> clusterA, List<DataUnit> clusterB)
		{
			List<DataUnit> bigger = clusterA.Count > clusterB.Count ? clusterA : clusterB;
			List<DataUnit> smaller = clusterA.Count > clusterB.Count ? clusterB : clusterA;
			double min = 800;
			if (bigger.Count < parallelFrom)
			{
				foreach (DataUnit a in bigger)
				{
					foreach (DataUnit b in smaller)
					{
						double d = GetEucledianDistance(a, b);
						if (d < min) min = d;
					}
				}
			}
			else
			{
				Parallel.ForEach(bigger, a =>
				{
					foreach (DataUnit b in smaller)
					{
						double d = GetEucledianDistance(a, b);
						if (d < min) min = d;
					}
				});
			}
			return min;
		}
	}
}
