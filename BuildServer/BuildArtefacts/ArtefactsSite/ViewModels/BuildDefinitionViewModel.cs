using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ArtefactsSite.ViewModels
{
    public class BuildDefinitionViewModel
    {
        public BuildDefinitionViewModel(string title, string anchor)
        {
            Title = title;
            Anchor = anchor;
        }

        public string Title { get; set; }
        public string Anchor { get; set; }

        public List<ArtefactViewModel> Artefacts { get; set; } 
    }
}