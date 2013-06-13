using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Linq;
using System.ServiceModel.Syndication;
using ArtefactsSite.Data;

namespace ArtefactsSite.Controllers
{
    public class RssController : Controller
    {
        public ActionResult For(string id)
        {
            var defs = MvcApplication.BuildDefinitions;
            var currentDef = defs.FirstOrDefault(d => 0 == String.Compare(d.Anchor, id, StringComparison.InvariantCultureIgnoreCase));

            if (null == currentDef)
            {
                return HttpNotFound();
            }

            var artefacts = ArtefactRepository.GetFileListing(currentDef.ArtefactQuery);

            var leftpart = Request.Url.GetLeftPart(UriPartial.Authority);

            var postItems = artefacts.Select(p => new SyndicationItem(p.FileName, "", new Uri(leftpart + Url.Content("~/" + p.FileName))));

            var feed = new SyndicationFeed(currentDef.Title, "SharpDevelop Build Server Automated Feed", new Uri(Request.Url.AbsoluteUri), postItems)
            {
                Copyright = new TextSyndicationContent("(c) SharpDevelop Team"),
                Language = "en-US"
            };

            return new FeedResult(new Rss20FeedFormatter(feed));
        }
    }
}
