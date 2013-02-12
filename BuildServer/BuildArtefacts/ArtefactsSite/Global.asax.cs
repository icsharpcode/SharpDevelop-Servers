using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Http;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;
using Newtonsoft.Json.Linq;
using ArtefactsSite.Models;

namespace ArtefactsSite
{
    // Note: For instructions on enabling IIS6 or IIS7 classic mode, 
    // visit http://go.microsoft.com/?LinkId=9394801

    public class MvcApplication : System.Web.HttpApplication
    {
        public static List<BuildDefinition> BuildDefinitions { get; private set; }

        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();

            WebApiConfig.Register(GlobalConfiguration.Configuration);
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);

            BuildDefinitions = GetBuildDefinitions();
        }

        public List<BuildDefinition> GetBuildDefinitions()
        {
            string json = File.ReadAllText(Server.MapPath("~/App_Data/BuildDefinitions.json"));
            var array = JObject.Parse(json);

            var list = new List<BuildDefinition>();
            foreach (var obj in array["buildDefinitions"])
            {
                var query = (string)obj["artefactQuery"];
                var title = (string)obj["title"];
                var anchor = (string)obj["anchor"];

                list.Add(new BuildDefinition() 
                { 
                    ArtefactQuery = query, 
                    Title = title, 
                    Anchor = anchor 
                });
            }

            return list;
        }
    }
}