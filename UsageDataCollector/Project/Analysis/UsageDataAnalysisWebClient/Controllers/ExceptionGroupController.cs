using System.Linq;
using System.Web.Mvc;
using UsageDataAnalysisWebClient.Models;
using UsageDataAnalysisWebClient.Repositories;

namespace UsageDataAnalysisWebClient.Controllers
{
	[Authorize]
    public class ExceptionGroupController : Controller
    {

		
        public ActionResult Index()
        {
        	ViewData.Model = new ExceptionGroupRepository().GetExceptionGroups();
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
            exceptionGroupRepository.Save(id, exceptionGroupEditModel.UserComment, exceptionGroupEditModel.UserFixedInRevision);
            return RedirectToAction("Index");
        }

    }
}
