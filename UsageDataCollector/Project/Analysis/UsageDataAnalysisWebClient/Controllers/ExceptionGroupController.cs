using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.UI.MobileControls;
using UsageDataAnalysisWebClient.Models;

namespace UsageDataAnalysisWebClient.Controllers
{
    public class ExceptionGroupController : Controller
    {
		private udcEntities _db;


        public ActionResult Index()
        {
			_db = new udcEntities();
			ViewData.Model = _db.ExceptionGroups.ToList();
            return View();
        }

		public ActionResult Edit(int id) {
			_db = new udcEntities();
			ViewData.Model = _db.ExceptionGroups.First(exceptionGroup => exceptionGroup.ExceptionGroupId == id);
			return View();
		}

		[AcceptVerbs(HttpVerbs.Post)]
		public ActionResult Edit(int id, FormCollection collection) {
			_db = new udcEntities();
			ExceptionGroup selectedExceptionGroup = _db.ExceptionGroups.First(exceptionGroup => exceptionGroup.ExceptionGroupId == id);
			ViewData.Model = selectedExceptionGroup;
			selectedExceptionGroup.UserComment = collection["UserComment"];
			selectedExceptionGroup.UserFixedInRevision = int.Parse(collection["UserFixedInRevision"]);
			_db.SaveChanges();
			return RedirectToAction("Index");
		}

    }
}
