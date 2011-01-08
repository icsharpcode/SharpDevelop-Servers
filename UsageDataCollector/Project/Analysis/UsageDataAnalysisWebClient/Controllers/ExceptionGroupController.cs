using System.Linq;
using System.Web.Mvc;
using UsageDataAnalysisWebClient.Models;
using UsageDataAnalysisWebClient.Repositories;

namespace UsageDataAnalysisWebClient.Controllers
{
	[Authorize]
    public class ExceptionGroupController : Controller
    {
        public ActionResult Index(ExceptionGroupIndexModel model)
        {
			ExceptionGroupRepository repo = new ExceptionGroupRepository();
			if (model.StartCommitHash == null)
				model.StartCommitHash = repo.GetLatestTagName();
			if (model.EndCommitHash == null)
				model.EndCommitHash = "";
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
            exceptionGroupRepository.Save(id, exceptionGroupEditModel.UserComment, exceptionGroupEditModel.UserFixedInCommit);
            return RedirectToAction("Index");
        }

    }
}
