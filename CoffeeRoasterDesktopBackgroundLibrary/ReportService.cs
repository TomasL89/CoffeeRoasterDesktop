using CoffeeRoasterDesktopBackground;
using CoffeeRoasterDesktopBackgroundLibrary.Data;
using LiteDB;
using System;
using System.Collections.Generic;
using System.Text;

namespace CoffeeRoasterDesktopBackgroundLibrary
{
    public class ReportService
    {
        private readonly Configuration configuration;
        private readonly ConfigurationService configurationService;

        public ReportService()
        {
            configurationService = new ConfigurationService();

            configuration = configurationService.SystemConfiguration;
        }

        public void SaveReportToDB(RoastReport roastReport)
        {
            try
            {
                using (var db = new LiteDatabase(configuration.LogFileDatabaseDirectory))
                {
                    var roastTable = db.GetCollection<RoastReport>("roastReport");
                    roastTable.Insert(roastReport);
                }
            }
            catch (Exception)
            {
            }
        }

        public IEnumerable<RoastReport> GetAllReports()
        {
            try
            {
                using (var db = new LiteDatabase(configuration.LogFileDatabaseDirectory))
                {
                    var roastReport = db.GetCollection<RoastReport>("roastReport");
                    return roastReport.Query().ToList();
                }
            }
            catch (Exception)
            {
                return null;
            }
        }

        public RoastReport LoadReportFromDB(Guid roastId)
        {
            try
            {
                using var db = new LiteDatabase(configuration.LogFileDatabaseDirectory);
                var roastLog = db.GetCollection<RoastReport>("roastReport");

                var result = roastLog.Query().Where(x => x.Id == roastId).SingleOrDefault();
                return result;
            }
            catch (Exception)
            {
                return null;
            }
        }
    }
}