using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using UsageDataAnalysisWebClient.Models;
using System.Data.Objects;
using System.Diagnostics;
using System.Data.SqlClient;
using System.Configuration;
using System.Data;

namespace UsageDataAnalysisWebClient.Repositories
{
	public class FeatureRepository
	{
		private udcEntities _db = new udcEntities();

		public IEnumerable<FeatureIndexEntry> GetFeatures(string featureNamePattern, int commitID)
		{
			var result = new List<FeatureIndexEntry>();
			using (var c = new SqlConnection(ConfigurationManager.ConnectionStrings["udcADO"].ConnectionString)) {
				c.Open();
				using (var command = c.CreateCommand()) {
					command.CommandText = "[FeatureIndex]";
					command.CommandType = CommandType.StoredProcedure;
					command.Parameters.Add("@featureNamePattern", SqlDbType.VarChar).Value = featureNamePattern;
					command.Parameters.Add("@commitID", SqlDbType.Int).Value = commitID;
					using (var reader = command.ExecuteReader()) {
						while (reader.Read()) {
							result.Add(new FeatureIndexEntry {
								FeatureID = reader.GetInt32(0),
								FeatureName = reader.GetString(1),
								TotalUseCount = reader.GetInt32(2),
								UserDays = reader.GetInt32(3)
							});
						}
					}
				}
			}
			return result;
		}

		private List<T> EvaluateQuery<T>(IQueryable<T> query)
		{
			Debug.WriteLine(((System.Data.Objects.ObjectQuery)query).ToTraceString());
			Stopwatch w = Stopwatch.StartNew();
			var list = query.ToList();
			Debug.WriteLine("Query took " + w.ElapsedMilliseconds + "ms and returned " + list.Count + " rows");
			return list;
		}
	}
}