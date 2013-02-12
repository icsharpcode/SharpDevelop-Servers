using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using ArtefactsSite.Data;
using ArtefactsSite.ViewModels;

namespace ArtefactsSite.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            var defs = MvcApplication.BuildDefinitions;

            var vm = new HomeIndexPageViewModel();

            foreach (var buildDefinition in defs)
            {
                var build = new BuildDefinitionViewModel(buildDefinition.Title, buildDefinition.Anchor);
                build.Artefacts = ArtefactRepository.GetFileListing(buildDefinition.ArtefactQuery);

                vm.Builds.Add(build);
            }

            vm.RenderInformation = "Rendered at: " + DateTime.Now.ToString() +
                ", .NET Version: " + Environment.Version;

            return View(vm);
        }

    }
}
