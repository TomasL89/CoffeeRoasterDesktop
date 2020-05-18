using CoffeeRoasterDesktopBackground;
using CoffeeRoasterDesktopBackgroundLibrary.Data;
using CoffeeRoasterDesktopBackgroundLibrary.Error;
using LiteDB;
using System;
using System.Collections.Generic;

namespace CoffeeRoasterDesktopBackgroundLibrary
{
    public class ReportService
    {
        private const string ROAST_REPORT_TABLE_NAME = "roastreport";

        private readonly Configuration configuration;
        private readonly ConfigurationService configurationService;

        public ReportService()
        {
            configurationService = new ConfigurationService();

            configuration = configurationService.SystemConfiguration;
        }

        public bool SaveReportToDB(RoastReport roastReport)
        {
            try
            {
                using var db = new LiteDatabase(configuration.LogFileDatabaseDirectory);
                var roastTable = db.GetCollection<RoastReport>(ROAST_REPORT_TABLE_NAME);
                var roastItem = roastTable.Query().Where(x => x.Id == roastReport.Id).SingleOrDefault();

                if (roastItem == null)
                    roastTable.Insert(roastReport);
                else
                    roastTable.Update(roastReport);

                return true;
            }
            catch (Exception ex)
            {
                ErrorService.LogError(SeverityLevel.Error, ErrorType.Database, $"Class {typeof(ReportService)} failed when attempting to create a roast report in table {ROAST_REPORT_TABLE_NAME}.\n    Log file database location:\n       {configuration.LogFileDatabaseDirectory}", ex);
                return false;
            }
        }

        public IEnumerable<RoastReport> GetAllReports()
        {
            try
            {
                using var db = new LiteDatabase(configuration.LogFileDatabaseDirectory);
                var roastReport = db.GetCollection<RoastReport>(ROAST_REPORT_TABLE_NAME);

                return roastReport.Query().ToList();
            }
            catch (Exception ex)
            {
                ErrorService.LogError(SeverityLevel.Error, ErrorType.Database, $"Class {typeof(ReportService)} failed when attempting to obtain all reports from {ROAST_REPORT_TABLE_NAME}.\n    Log file database location:\n       {configuration.LogFileDatabaseDirectory}", ex);
                return null;
            }
        }

        public RoastReport LoadReportFromDB(Guid roastId)
        {
            try
            {
                using var db = new LiteDatabase(configuration.LogFileDatabaseDirectory);
                var roastLog = db.GetCollection<RoastReport>(ROAST_REPORT_TABLE_NAME);

                var result = roastLog.Query().Where(x => x.Id == roastId).SingleOrDefault();
                return result;
            }
            catch (Exception ex)
            {
                ErrorService.LogError(SeverityLevel.Error, ErrorType.Database, $"Class {typeof(ReportService)} failed when attempting to load a report with ID {roastId} .\n    Log file database location:\n       {configuration.LogFileDatabaseDirectory}", ex);
                return null;
            }
        }
    }
}