using ReliefLib;
using ReliefWeb.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http.Formatting;
using System.Threading.Tasks;
using System.Web;
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

					List<DataUnit> data = DataPreparator.PrepareData(lines.ToArray(), options);

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

					List<string> keys;
					if (options.SkipFirstLine)
					{
						keys = DataPreparator.GetColumnDefinitions(lines.ToArray(), options);
					}
					else
					{
						keys = new List<string>();
						for (int i = 0; i < data[0].Columns.Count; i++)
						{
							keys.Add(i.ToString());
						}
					}

					List<ReliefResult> results = new List<ReliefResult>();
					foreach (IReliefAlgorithm relief in reliefs)
					{
						results.Add(ParseResult(relief, keys, sort != null));
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

		private ReliefResult ParseResult(IReliefAlgorithm relief, List<string> keys, bool sort)
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

			if (sort)
				result.Scores = result.Scores.OrderByDescending(x => x.Value).ToList();
			if (result.Scores.Count > 1000)
			{
				result.RemovedCount = result.Scores.Count - 1000;
				result.Scores.RemoveRange(1000, result.Scores.Count - 1000);
			}

			return result;
		}
	}
}