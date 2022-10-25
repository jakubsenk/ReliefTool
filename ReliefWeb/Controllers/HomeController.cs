using ReliefLib;
using ReliefWeb.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net.Http.Formatting;
using System.Threading.Tasks;
using System.Web;
using System.Web.Helpers;
using System.Web.Mvc;

namespace ReliefWeb.Controllers
{
	public class HomeController : Controller
	{
		public ActionResult Index()
		{
			return View();
		}

		[HttpPost]
		public ActionResult Relief()
		{
			HttpPostedFileBase file = Request.Files["file"];
			string skip = Request.Form.Get("skip");
			string sresultClass = Request.Form.Get("resultclass");
			string sNormalize = Request.Form.Get("normalize");
			string separator = Request.Form.Get("separator");
			string parallel = Request.Form.Get("parallel");
			string sk = Request.Form.Get("k");
			string sort = Request.Form.Get("sort");
			string cluster = Request.Form.Get("cluster");
			string clusterCount = Request.Form.Get("clusterCount");
			string clusterProperties = Request.Form.Get("clusterProps");

			if (file == null)
			{
				return View("Index", (object)"All fields are required!");
			}

			if (string.IsNullOrEmpty(separator))
			{
				return View("Index", (object)"Separator is missing!");
			}

			if (GlobalStore.CalculationPending)
			{
				return View("Index", (object)"Only one calculation can run at the same time.");
			}

			GlobalStore.CalculationPending = true;

			try
			{
				int k = int.Parse(sk);
				using (StreamReader sr = new StreamReader(file.InputStream))
				{
					List<string> lines = new List<string>();
					while (!sr.EndOfStream)
					{
						lines.Add(sr.ReadLine());
					}
					DataPreparatorOptions options = new DataPreparatorOptions()
					{
						ColumnSeparator = separator[0],
						SkipFirstLine = skip != null,
						ResultClassIsFirstColumn = sresultClass == "0",
						Normalize = sNormalize != null
					};

					PreparatorResult result = DataPreparator.PrepareData(lines.ToArray(), options);
					List<DataUnit> data = result.Data;

					List<IReliefAlgorithm> reliefs = new List<IReliefAlgorithm>();
					reliefs.Add(new ReliefJava());
					reliefs.Add(new Relief());
					reliefs.Add(new ReliefConsistent());
					reliefs.Add(new ReliefF(k));
					reliefs.Add(new ReliefFConsistent(k));

					if (parallel != null)
					{
						Parallel.ForEach(reliefs, (relief) =>
						{
							relief.ProccessData(data);
						});
					}
					else
					{
						foreach (IReliefAlgorithm relief in reliefs)
						{
							relief.ProccessData(data);
						}
					}

					List<ReliefResult> results = new List<ReliefResult>();
					foreach (IReliefAlgorithm relief in reliefs)
					{
						results.Add(ParseResult(relief, result.Keys));
					}

					if (!string.IsNullOrEmpty(cluster))
					{
						Random r = new Random();
						foreach (ReliefResult item in results)
						{
							item.Graphs = new List<GraphModel>();

							List<KeyValuePair<string, double>> clusterTemp = item.SortedScores.Take(int.Parse(clusterProperties)).ToList();
							List<int> clusterProps = new List<int>();
							foreach (KeyValuePair<string, double> cl in clusterTemp)
							{
								clusterProps.Add(result.Keys.IndexOf(cl.Key));
							}
							IClusterer c = new AlgLibClusterer();
							item.Clusters = c.GetClusters(data, int.Parse(clusterCount), clusterProps);

							item.Sillhoutte = SillhouteIndex.GetIndex(item.Clusters);

							List<Tuple<int, int>> pairs = GetClusterPropertiesPairs(result, item, int.Parse(clusterProperties));
							for (int j = 0; j < pairs.Count; j++)
							{
								item.Graphs.Add(new GraphModel());
								item.Graphs[j].XAxis = result.Keys[pairs[j].Item1];
								item.Graphs[j].YAxis = result.Keys[pairs[j].Item2];
								for (int i = 0; i < item.Clusters.Rows.Count; i++)
								{
									item.Graphs[j].ClusterPairsX.Add(new List<double>());
									item.Graphs[j].ClusterPairsY.Add(new List<double>());
									item.Graphs[j].ClusterColors.Add(Color.FromArgb(r.Next(256), r.Next(256), r.Next(256)));
									foreach (DataUnit dataUnit in item.Clusters.Rows[i].ClusterData)
									{
										item.Graphs[j].ClusterPairsX[i].Add(dataUnit.Columns[pairs[j].Item1].NumericValue);
										item.Graphs[j].ClusterPairsY[i].Add(dataUnit.Columns[pairs[j].Item2].NumericValue);
									}
								}
							}
						}
					}

					data.Clear();
					if (sort != null)
						return View("ReliefSorted", results);
					else
						return View(results);
				}
			}
			catch (Exception ex)
			{
				return View("Index", (object)ex.Message);
			}
			finally
			{
				GlobalStore.CalculationPending = false;
			}
		}

		private ReliefResult ParseResult(IReliefAlgorithm relief, List<string> keys)
		{
			List<KeyValuePair<string, double>> resultList = new List<KeyValuePair<string, double>>();
			for (int i = 0; i < relief.Scores.Count; i++)
			{
				resultList.Add(new KeyValuePair<string, double>(keys[i], relief.Scores[i]));
			}

			ReliefResult result = new ReliefResult()
			{
				Scores = resultList,
				Duration = relief.Elapsed,
				Name = relief.GetType().Name
			};

			double max = resultList.Max(x => x.Value);
			result.BestScore = resultList.Where(x => x.Value == max).First();

			result.SortedScores = result.Scores.OrderByDescending(x => x.Value).ToList();
			if (result.Scores.Count > 1000)
			{
				result.RemovedCount = result.Scores.Count - 1000;
				result.Scores.RemoveRange(1000, result.Scores.Count - 1000);
			}

			return result;
		}

		private List<Tuple<int, int>> GetClusterPropertiesPairs(PreparatorResult result, ReliefResult item, int maxScores)
		{
			List<KeyValuePair<string, double>> clusterTemp = item.SortedScores.Take(maxScores).ToList();
			List<int> clusterProps = new List<int>();
			foreach (KeyValuePair<string, double> cl in clusterTemp)
			{
				clusterProps.Add(result.Keys.IndexOf(cl.Key));
			}

			HashSet<Tuple<int, int>> res = new HashSet<Tuple<int, int>>();
			foreach (int outer in clusterProps)
			{
				foreach (int inner in clusterProps)
				{
					if (inner == outer) continue;
					if (!res.Contains(new Tuple<int, int>(outer, inner)) && !res.Contains(new Tuple<int, int>(inner, outer)))
						res.Add(new Tuple<int, int>(outer, inner));
				}
			}

			return res.ToList();
		}
	}
}