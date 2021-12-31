using ReliefLib;
using ReliefWeb.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http.Formatting;
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
			string separator = Request.Form.Get("separator");
			string parallel = Request.Form.Get("parallel");

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
						ResultClassIsFirstColumn = sresultClass == "0"
					};

					List<DataUnit> data = DataPreparator.PrepareData(lines.ToArray(), options);

					Stopwatch sw = new Stopwatch();
					sw.Start();

					Relief r = new Relief(data, parallel != null);

					sw.Stop();

					List<string> keys;
					if (options.SkipFirstLine)
					{
						keys = DataPreparator.GetColumnDefinitions(lines.ToArray(), options);
					}
					else
					{
						keys = new List<string>();
						for (int i = 0; i < r.Scores.Count; i++)
						{
							keys.Add(i.ToString());
						}
					}
					List<KeyValuePair<string, double>> resultList = new List<KeyValuePair<string, double>>();
					for (int i = 0; i < r.Scores.Count; i++)
					{
						resultList.Add(new KeyValuePair<string, double>(keys[i], r.Scores[i]));
					}

					ReliefResult result = new ReliefResult()
					{
						Scores = resultList,
						Duration = sw.Elapsed
					};

					double max = resultList.Max(x => x.Value);
					result.BestScore = resultList.Where(x => x.Value == max).First();

					result.Scores.Sort((a, b) => (int)(b.Value * 100 - a.Value * 100));
					if (result.Scores.Count > 1000)
					{
						result.RemovedCount = result.Scores.Count - 1000;
						result.Scores.RemoveRange(1000, result.Scores.Count - 1000);
					}

					data.Clear();
					GlobalStore.CalculationPending = false;
					return View(result);
				}
			}
			catch (Exception ex)
			{
				GlobalStore.CalculationPending = false;
				return View("Index", (object)ex.Message);
			}
		}
	}
}