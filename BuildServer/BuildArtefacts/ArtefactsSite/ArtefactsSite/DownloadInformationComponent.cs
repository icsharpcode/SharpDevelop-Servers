using System.Configuration;
using System.IO;
using System.Collections.Generic;
using System.Web.Caching;
using System.Web;
using System.Linq;
using System.Collections;

public class DownloadInformationComponent
{
    public DownloadInformationComponent()
    {
    }

    public List<DownloadInformation> GetFileListing(string artefactQuery, string sortExpression)
    {
        string cacheKey = artefactQuery + "CacheKey";
        object diCacheItem = HttpContext.Current.Cache[cacheKey];
        List<DownloadInformation> files = null;

        if (null != diCacheItem)
        {
            // copy for thread-safe sorting
            files = new List<DownloadInformation>((List<DownloadInformation>)diCacheItem);
        }
        else
        {
            string artefactDirectoryAbsolute =
                HttpContext.Current.Request.MapPath(ConfigurationManager.AppSettings["artefactsDirectory"]);

            /* pre-.NET 4.0
            string[] fileNames = Directory.GetFiles(
                artefactDirectoryAbsolute,
                artefactQuery);

            var diQuery = from fn in fileNames
                          select new DownloadInformation() { FileName = Path.GetFileName(fn), InternalCreationDate = File.GetCreationTime(fn) };
            */
            
            var diQuery = from fn in Directory.EnumerateFiles(artefactDirectoryAbsolute, artefactQuery)
                          select new DownloadInformation() { FileName = Path.GetFileName(fn), InternalCreationDate = File.GetCreationTime(fn) };

            files = diQuery.OrderByDescending(f => f.InternalCreationDate).ToList();

            HttpContext.Current.Cache.Insert(cacheKey,
                new List<DownloadInformation>(files),
                new CacheDependency(artefactDirectoryAbsolute));
        }

        if (sortExpression.Length > 0)
        {
            bool sortDescending = sortExpression.ToLowerInvariant().EndsWith(" desc");
            var query = from f in files select f;

            if (sortDescending)
            {
                string sortColumn = sortExpression.Substring(0, sortExpression.Length - 5);
                if ("FileName" == sortColumn)
                    files = query.OrderByDescending(f => f.FileName).ToList();
                else
                    files = query.OrderByDescending(f => f.InternalCreationDate).ToList();
            }
            else
            {
                string sortColumn = sortExpression;
                if ("FileName" == sortColumn)
                    files = query.OrderBy(f => f.FileName).ToList();
                else
                    files = query.OrderBy(f => f.InternalCreationDate).ToList();
            }
        }

        return files;
    }
}