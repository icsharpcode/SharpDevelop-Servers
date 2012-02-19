using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using UsageDataAnalysisWebClient.Models;
using UsageDataAnalysisWebClient.Repositories;

namespace UsageDataAnalysisWebClient.Controllers
{
	[Authorize]
    public class FeatureController : Controller
    {
        //
        // GET: /Feature/
		
		public ActionResult Index(FeatureIndexModel model)
		{
			if (model.FeatureFilter != null && model.VersionFilter != null) {
				int? commitID = SourceControlRepository.FindCommitId(model.VersionFilter);
				if (commitID == null) {
					model.ErrorMessage = "Unknown version: " + model.VersionFilter;
				} else {
					model.Entries = new FeatureRepository().GetFeatures(model.FeatureFilter, commitID.Value);
				}
			}
			if (model.VersionFilter == null) {
				model.VersionFilter = SourceControlRepository.GetLatestTagName(14);
			}
			if (model.FeatureFilter == null) {
				model.FeatureFilter = "%";
			}
			return View(model);
		}
    }
}
