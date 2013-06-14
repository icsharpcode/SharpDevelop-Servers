using System.Linq;
using System.Web.Mvc;
using UsageDataAnalysisWebClient.Models;
using UsageDataAnalysisWebClient.Repositories;
using System.IO;

namespace UsageDataAnalysisWebClient.Controllers
{
	[Authorize]
    public class ExceptionGroupController : Controller
    {
        public ActionResult Index(ExceptionGroupIndexModel model)
        {
			if (model.StartCommitHash == null)
				model.StartCommitHash = SourceControlRepository.GetLatestTagName(14);
			if (model.EndCommitHash == null)
				model.EndCommitHash = "";
			ExceptionGroupRepository repo = new ExceptionGroupRepository();
			model.Entries = repo.GetExceptionGroups(model.StartCommitHash, model.EndCommitHash);
			ViewData.Model = model;
            return View();
        }

		public ActionResult Edit(int id) {
			ViewData.Model = new ExceptionGroupRepository().GetExceptionGroupById(id);
			return View();
		}

        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult Edit(int id, ExceptionGroupEditModel exceptionGroupEditModel)
        {
        	ExceptionGroupRepository exceptionGroupRepository = new ExceptionGroupRepository();
            exceptionGroupRepository.Save(id, exceptionGroupEditModel.UserComment, exceptionGroupEditModel.UserFixedInCommitHash);
			return RedirectToAction("Edit", id);
        }

		public ActionResult CrashProbabilityChart(int id)
		{
			var chart = new System.Web.UI.DataVisualization.Charting.Chart();
			var crashes = chart.Series.Add("Occurrences");
			crashes.ChartType = System.Web.UI.DataVisualization.Charting.SeriesChartType.Line;

			ExceptionGroupRepository exceptionGroupRepository = new ExceptionGroupRepository();
			foreach (var pair in exceptionGroupRepository.GetCrashStatisticsForExceptionGroup(id)) {
				crashes.Points.AddXY(pair.Item1, pair.Item2);
			}

			chart.Width = 800;
			chart.Height = 300;
			chart.RenderType = System.Web.UI.DataVisualization.Charting.RenderType.ImageTag;
			chart.ChartAreas.Add("Series 1").AxisX.Interval = 1;

			using (var ms = new MemoryStream()) {
				chart.SaveImage(ms, System.Web.UI.DataVisualization.Charting.ChartImageFormat.Png);
				ms.Position = 0;
				return File(ms.ToArray(), "image/png", "mychart.png");
			}
		}
    }
}
