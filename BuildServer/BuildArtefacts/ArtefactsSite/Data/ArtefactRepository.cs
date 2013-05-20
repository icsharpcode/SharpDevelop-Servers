using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Caching;
using ArtefactsSite.ViewModels;

namespace ArtefactsSite.Data
{
    public static class ArtefactRepository
    {
        public static List<ArtefactViewModel> GetFileListing(string artefactQuery)
        {
            string cacheKey = artefactQuery + "CacheKey";
            object diCacheItem = HttpContext.Current.Cache[cacheKey];
            List<ArtefactViewModel> files = null;

            if (null != diCacheItem)
            {
                return diCacheItem as List<ArtefactViewModel>;
            }
            else
            {
                string artefactDirectoryAbsolute = HttpContext.Current.Request.MapPath("~/");

                var diQuery = from fn in Directory.EnumerateFiles(artefactDirectoryAbsolute, artefactQuery)
                              select new ArtefactViewModel()
                                  {
                                      FileName = Path.GetFileName(fn), 
                                      InternalCreationDate = File.GetCreationTime(fn)
                                  };

                files = diQuery.OrderByDescending(f => f.InternalCreationDate).ToList();

                HttpContext.Current.Cache.Insert(cacheKey,
                    new List<ArtefactViewModel>(files),
                    new CacheDependency(artefactDirectoryAbsolute));
            }

            return files;
        }
    }
}