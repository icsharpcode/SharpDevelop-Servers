using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using UsageDataAnalysisWebClient.Models;

namespace UsageDataAnalysisWebClient.Controllers
{
    public class SessionController : Controller
    {
    	private udcEntities _db;

        public ActionResult Index()
        {
			_db = new udcEntities();
        	ViewData.Model = _db.Sessions.ToList();
            return View();
        }

    }
}
