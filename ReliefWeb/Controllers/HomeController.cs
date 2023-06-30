using PdfSharp.Pdf;
using ReliefLib;
using ReliefLib.AlgLib;
using ReliefWeb.Models;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using PdfSharp.Drawing;
using PdfSharp.Drawing.Layout;

namespace ReliefWeb.Controllers
{
	public class HomeController : Controller
	{
		private static List<ReliefResult> exportData = null;

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
			string clusterType = Request.Form.Get("cluster-type");
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
							item.GraphsAll = new List<GraphModel>();

							List<KeyValuePair<string, double>> clusterTemp = item.SortedScores.Take(int.Parse(clusterProperties)).ToList();
							List<int> clusterProps = new List<int>();
							foreach (KeyValuePair<string, double> cl in clusterTemp)
							{
								clusterProps.Add(result.Keys.IndexOf(cl.Key));
							}
							IClusterer c = GetClusterer(clusterType);
							item.Clusters = c.GetClusters(data, int.Parse(clusterCount), clusterProps);

							clusterProps.Clear();
							for (int i = 0; i < result.Keys.Count; i++)
							{
								clusterProps.Add(i);
							}

							item.ClustersAll = c.GetClusters(data, int.Parse(clusterCount), clusterProps);

							List<Color> colors = new List<Color>();
							for (int i = 0; i < int.Parse(clusterCount); i++)
							{
								colors.Add(Color.FromArgb(r.Next(256), r.Next(256), r.Next(256)));
							}

							List<List<double>> sillhoute = SillhouteIndex.GetIndex(item.Clusters);
							SillhouteModel model = new SillhouteModel();
							for (int i = 0; i < sillhoute.Count; i++)
							{
								model.ClusterPairs.Add(sillhoute[i]);
								model.ClusterColors.Add(colors[i]);
							}
							item.Sillhoutte = model;

							sillhoute = SillhouteIndex.GetIndex(item.ClustersAll);
							model = new SillhouteModel();
							for (int i = 0; i < sillhoute.Count; i++)
							{
								model.ClusterPairs.Add(sillhoute[i]);
								model.ClusterColors.Add(colors[i]);
							}
							item.SillhoutteAll = model;

							List<Tuple<int, int>> pairs = GetClusterPropertiesPairs(result, item, int.Parse(clusterProperties));
							for (int j = 0; j < pairs.Count; j++)
							{
								GraphModel graph = new GraphModel();
								GraphModel graphOrig = new GraphModel();

								graph.XAxis = result.Keys[pairs[j].Item1];
								graph.YAxis = result.Keys[pairs[j].Item2];

								graphOrig.XAxis = result.Keys[pairs[j].Item1];
								graphOrig.YAxis = result.Keys[pairs[j].Item2];

								for (int i = 0; i < item.Clusters.Rows.Count; i++)
								{
									Color col = colors[i];
									graph.ClusterPairsX.Add(new List<double>());
									graph.ClusterPairsY.Add(new List<double>());
									graph.ClusterColors.Add(col);
									foreach (DataUnit dataUnit in item.Clusters.Rows[i].ClusterData)
									{
										graph.ClusterPairsX[i].Add(result.OriginalValues[dataUnit].Columns[pairs[j].Item1].NumericValue);
										graph.ClusterPairsY[i].Add(result.OriginalValues[dataUnit].Columns[pairs[j].Item2].NumericValue);
									}

									graphOrig.ClusterPairsX.Add(new List<double>());
									graphOrig.ClusterPairsY.Add(new List<double>());
									graphOrig.ClusterColors.Add(col);

									foreach (DataUnit dataUnit in item.ClustersAll.Rows[i].ClusterData)
									{
										graphOrig.ClusterPairsX[i].Add(result.OriginalValues[dataUnit].Columns[pairs[j].Item1].NumericValue);
										graphOrig.ClusterPairsY[i].Add(result.OriginalValues[dataUnit].Columns[pairs[j].Item2].NumericValue);
									}
								}
								item.Graphs.Add(graph);
								item.GraphsAll.Add(graphOrig);
							}
						}
					}

					data.Clear();
					exportData = results;
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

		private IClusterer GetClusterer(string clusterType)
		{
			switch (clusterType)
			{
				case "AlgLib.Ahc": return new AlgLibAhcClusterer();
				case "AlgLib.Kmeans": return new AlgLibKmeansClusterer();
				case "Internal": return new InternalClusterer();
				default: throw new ArgumentOutOfRangeException(nameof(clusterType), clusterType, "Invalid cluster type.");
			}
		}

		public ActionResult Chart(GraphModel model)
		{
			var a = RouteData.Values;
			return View(model);
		}

		[HttpGet]
		public ActionResult Export()
		{
			PdfDocument pdf = new PdfDocument();
			PdfPage page = pdf.AddPage();
			XGraphics gfx = XGraphics.FromPdfPage(page);
			XFont font = new XFont("Verdana", 20, XFontStyle.Bold);

			XSolidBrush rect_style1 = new XSolidBrush(XColors.LightGray);
			XSolidBrush rect_style2 = new XSolidBrush(XColors.DarkGreen);

			int marginLeft = 20;
			int marginTop = 40;
			int el_height = 30;
			int rect_height = 17;

			int el1_width = 80;
			int el2_width = 380;
			int offSetX_1 = el1_width;
			int interLine_X_1 = 2;

			XStringFormat format = new XStringFormat();
			format.LineAlignment = XLineAlignment.Near;
			format.Alignment = XStringAlignment.Near;

			gfx.DrawRectangle(rect_style2, marginLeft, marginTop, el1_width * 2 + interLine_X_1, rect_height);

			var tf = new XTextFormatter(gfx);
			XFont fontParagraph = new XFont("Verdana", 8, XFontStyle.Regular);
			tf.DrawString("Algorithm", fontParagraph, XBrushes.White,
										new XRect(marginLeft, marginTop, el1_width, el_height), format);

			tf.DrawString("Best column", fontParagraph, XBrushes.White,
										new XRect(marginLeft + offSetX_1 + interLine_X_1, marginTop, el2_width, el_height), format);

			double lineHeight = 20;

			for (int p = 0; p < exportData.Count; p++)
			{
				double dist_Y = lineHeight * (p + 1);
				double dist_Y2 = dist_Y - 2;

				gfx.DrawRectangle(rect_style1, marginLeft, marginTop + dist_Y2, el1_width, rect_height);
				tf.DrawString(
						exportData[p].Name,
						fontParagraph,
						XBrushes.Black,
						new XRect(marginLeft, marginTop + dist_Y, el1_width, el_height),
						format);

				gfx.DrawRectangle(rect_style1, marginLeft + offSetX_1, marginTop + dist_Y2, el1_width, rect_height);
				tf.DrawString(
						exportData[p].BestScore.Key,
						fontParagraph,
						XBrushes.Black,
						new XRect(marginLeft + offSetX_1, marginTop + dist_Y, el1_width, el_height),
						format);
			}
			for (int p = 0; p < exportData.Count; p++)
			{
				// Page Options
				PdfPage pdfPage = pdf.AddPage();
				pdfPage.Height = 842;//842
				pdfPage.Width = 590;

				// Get an XGraphics object for drawing
				XGraphics graph = XGraphics.FromPdfPage(pdfPage);

				// Text format
				tf = new XTextFormatter(graph);


				// Row elements

				// page structure options


				int interLine_X_2 = 2 * interLine_X_1;

				int offSetX_2 = el1_width + el2_width;


				graph.DrawString(exportData[p].Name, font, XBrushes.Black, new XRect(10, 10, page.Width, page.Height), XStringFormats.TopLeft);

				for (int i = 0, c = 0; i < exportData[p].SortedScores.Take(1000).Count(); i++, c++)
				{
					double dist_Y = lineHeight * (c + 1);
					double dist_Y2 = dist_Y - 2;

					if (dist_Y > pdfPage.Height - 80)
					{
						pdfPage = pdf.AddPage();
						pdfPage.Height = 842;
						pdfPage.Width = 590;
						graph = XGraphics.FromPdfPage(pdfPage);

						// Text format
						tf = new XTextFormatter(graph);
						c = 0;
						dist_Y = lineHeight * (c + 1);
						dist_Y2 = dist_Y - 2;
					}
					// header della G
					if (i == 0)
					{
						graph.DrawRectangle(rect_style2, marginLeft, marginTop, el1_width + el2_width + interLine_X_1, rect_height);

						tf.DrawString("Attribute", fontParagraph, XBrushes.White,
													new XRect(marginLeft, marginTop, el1_width, el_height), format);

						tf.DrawString("Score", fontParagraph, XBrushes.White,
													new XRect(marginLeft + offSetX_1 + interLine_X_1, marginTop, el2_width, el_height), format);

						//tf.DrawString("column3", fontParagraph, XBrushes.White,
						//new XRect(marginLeft + offSetX_2 + 2 * interLine_X_2, marginTop, el1_width, el_height), format);

						// stampo il primo elemento insieme all'header
						//graph.DrawRectangle(rect_style1, marginLeft, dist_Y2 + marginTop, el1_width, rect_height);
						//tf.DrawString("text1", fontParagraph, XBrushes.Black,
						//							new XRect(marginLeft, dist_Y + marginTop, el1_width, el_height), format);

						////ELEMENT 2 - BIG 380
						//graph.DrawRectangle(rect_style1, marginLeft + offSetX_1 + interLine_X_1, dist_Y2 + marginTop, el2_width, rect_height);
						//tf.DrawString(
						//		"text2",
						//		fontParagraph,
						//		XBrushes.Black,
						//		new XRect(marginLeft + offSetX_1 + interLine_X_1, dist_Y + marginTop, el2_width, el_height),
						//		format);


						////ELEMENT 3 - SMALL 80

						//graph.DrawRectangle(rect_style1, marginLeft + offSetX_2 + interLine_X_2, dist_Y2 + marginTop, el1_width, rect_height);
						//tf.DrawString(
						//		"text3",
						//		fontParagraph,
						//		XBrushes.Black,
						//		new XRect(marginLeft + offSetX_2 + 2 * interLine_X_2, dist_Y + marginTop, el1_width, el_height),
						//		format);
					}

					//if (i % 2 == 1)
					//{
					//  graph.DrawRectangle(TextBackgroundBrush, marginLeft, lineY - 2 + marginTop, pdfPage.Width - marginLeft - marginRight, lineHeight - 2);
					//}

					//ELEMENT 1 - SMALL 80
					graph.DrawRectangle(rect_style1, marginLeft, marginTop + dist_Y2, el1_width, rect_height);
					tf.DrawString(

							exportData[p].SortedScores[i].Key,
							fontParagraph,
							XBrushes.Black,
							new XRect(marginLeft, marginTop + dist_Y, el1_width, el_height),
							format);

					//ELEMENT 2 - BIG 380
					graph.DrawRectangle(rect_style1, marginLeft + offSetX_1 + interLine_X_1, dist_Y2 + marginTop, el2_width, rect_height);
					tf.DrawString(
							exportData[p].SortedScores[i].Value.ToString("N5"),
							fontParagraph,
							XBrushes.Black,
							new XRect(marginLeft + offSetX_1 + interLine_X_1, marginTop + dist_Y, el2_width, el_height),
							format);


					//ELEMENT 3 - SMALL 80

					//graph.DrawRectangle(rect_style1, marginLeft + offSetX_2 + interLine_X_2, dist_Y2 + marginTop, el1_width, rect_height);
					//tf.DrawString(
					//		"text3",
					//		fontParagraph,
					//		XBrushes.Black,
					//		new XRect(marginLeft + offSetX_2 + 2 * interLine_X_2, marginTop + dist_Y, el1_width, el_height),
					//		format);
				}
			}

			using (MemoryStream ms = new MemoryStream())
			{
				pdf.Save(ms);
				return File(ms.ToArray(), "application/pdf", "export.pdf");
			}
		}
	}
}