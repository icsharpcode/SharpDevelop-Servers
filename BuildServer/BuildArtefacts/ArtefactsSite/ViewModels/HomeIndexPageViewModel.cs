using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ArtefactsSite.ViewModels
{
    public class HomeIndexPageViewModel
    {
        public HomeIndexPageViewModel()
        {
            Builds = new List<BuildDefinitionViewModel>();
        }

        public List<BuildDefinitionViewModel> Builds { get; set; }
        public string RenderInformation { get; set; }
    }
}