using System.Linq;
using System.Web.Mvc;
using UsageDataAnalysisWebClient.Models;

namespace UsageDataAnalysisWebClient.Controllers
{
	[Authorize]
    public class ExceptionGroupController : Controller
    {
		private udcEntities _db;

		
        public ActionResult Index()
        {
            const int minimumAffectedUsers = 2;
            const int minimumRevision = 5570; // TODO: make this parameter user-editable

			_db = new udcEntities();
            var q = from ex in _db.Exceptions
                    // filter exceptions:
                    // don't show follow-up errors 
                    where ex.IsFirstInSession
                    // ignore errors in ancient versions of the app
                    where ex.Session.AppVersionRevision >= minimumRevision
                    group ex by ex.ExceptionGroup into g
                    // search "first seen" and "last seen" in all exceptions within the exception group
                    // (not just those matching the filters above)
                    let sessionsForVersionSearch = g.Key.Exceptions.Select(ex => ex.Session).Where(s => s.AppVersionRevision > 0)
                    let firstSeenVersion = sessionsForVersionSearch.FirstOrDefault(s => s.AppVersionRevision == sessionsForVersionSearch.Min(s2 => s2.AppVersionRevision))
                    let lastSeenVersion = sessionsForVersionSearch.FirstOrDefault(s => s.AppVersionRevision == sessionsForVersionSearch.Max(s2 => s2.AppVersionRevision))
                    // fill the ViewModel instance with all the data
                    select new ExceptionGroupViewModel
                    {
                        ExceptionGroupId = g.Key.ExceptionGroupId,
                        ExceptionType = g.Key.ExceptionType,
                        ExceptionLocation = g.Key.ExceptionLocation,
                        UserComment = g.Key.UserComment,
                        UserFixedInRevision = g.Key.UserFixedInRevision,
                        AffectedUsers = g.Select(ex => ex.Session.UserId).Distinct().Count(),
                        Occurrences = g.Count(),
                        FirstSeenMajor = firstSeenVersion.AppVersionMajor,
                        FirstSeenMinor = firstSeenVersion.AppVersionMinor,
                        FirstSeenBuild = firstSeenVersion.AppVersionBuild,
                        FirstSeenRevision = firstSeenVersion.AppVersionRevision,
                        LastSeenMajor = lastSeenVersion.AppVersionMajor,
                        LastSeenMinor = lastSeenVersion.AppVersionMinor,
                        LastSeenBuild = lastSeenVersion.AppVersionBuild,
                        LastSeenRevision = lastSeenVersion.AppVersionRevision,
                    } into viewModel
                    where viewModel.AffectedUsers >= minimumAffectedUsers
                    orderby viewModel.AffectedUsers descending, viewModel.Occurrences descending
                    select viewModel;
			ViewData.Model = q.ToList();
            return View();
        }

		public ActionResult Edit(int id) {
			_db = new udcEntities();
			ViewData.Model = _db.ExceptionGroups.First(exceptionGroup => exceptionGroup.ExceptionGroupId == id);
			return View();
		}

        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult Edit(int id, FormCollection collection)
        {
            _db = new udcEntities();
            ExceptionGroup selectedExceptionGroup = _db.ExceptionGroups.First(exceptionGroup => exceptionGroup.ExceptionGroupId == id);
            ViewData.Model = selectedExceptionGroup;
            selectedExceptionGroup.UserComment = collection["UserComment"];
            if (string.IsNullOrWhiteSpace(collection["UserFixedInRevision"]))
                selectedExceptionGroup.UserFixedInRevision = null;
            else
                selectedExceptionGroup.UserFixedInRevision = int.Parse(collection["UserFixedInRevision"]);
            _db.SaveChanges();
            return RedirectToAction("Index");
        }

    }
}
