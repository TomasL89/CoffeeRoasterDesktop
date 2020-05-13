using CoffeeRoasterDesktopBackground;
using CoffeeRoasterDesktopBackgroundLibrary.Data;
using LiteDB;
using System;
using CoffeeRoasterDesktopBackgroundLibrary.Error;
using System.Collections.Generic;

namespace CoffeeRoasterDesktopBackgroundLibrary
{
    public class DataLogger
    {
        private const string ROAST_DATABASE_TABLE_NAME = "roast";
        private const string ROAST_LOG_DATABASE_TABLE_NAME = "roastlogs";

        private readonly Configuration configuration;
        private readonly ConfigurationService configurationService;

        public DataLogger()
        {
            configurationService = new ConfigurationService();

            configuration = configurationService.SystemConfiguration;
        }

        public Guid CreateLog()
        {
            try
            {
                using var db = new LiteDatabase(configuration.LogFileDatabaseDirectory);
                var roast = new Roast();
                var roastTable = db.GetCollection<Roast>(ROAST_DATABASE_TABLE_NAME);
                roastTable.Insert(new Roast());

                return roast.RoastId;
            }
            catch (Exception ex)
            {
                ErrorService.LogError(SeverityLevel.Error, ErrorType.Database, $"Class {typeof(DataLogger)} failed when attempting to create a log in table {ROAST_DATABASE_TABLE_NAME}.\n    Log file database location:\n       {configuration.LogFileDatabaseDirectory}", ex);
                return Guid.Empty;
            }
        }

        public bool SaveToDatabase(RoastLog roastLogRow)
        {
            try
            {
                using var db = new LiteDatabase(configuration.LogFileDatabaseDirectory);
                var roastLogs = db.GetCollection<RoastLog>(ROAST_LOG_DATABASE_TABLE_NAME);

                roastLogs.Insert(roastLogRow);
                return true;
            }
            catch (Exception ex)
            {
                ErrorService.LogError(SeverityLevel.Error, ErrorType.Database, $"Class {typeof(DataLogger)} failed when attempting to save a log file to database in table {ROAST_LOG_DATABASE_TABLE_NAME}.\n    Log file database location:\n       {configuration.LogFileDatabaseDirectory}", ex);
                return false;
            }
        }

        public IEnumerable<RoastLog> GetRoastById(Guid roastId)
        {
            try
            {
                using var db = new LiteDatabase(configuration.LogFileDatabaseDirectory);
                var roastLog = db.GetCollection<RoastLog>(ROAST_LOG_DATABASE_TABLE_NAME);

                return roastLog.Query().Where(x => x.RoastId == roastId).ToList();
            }
            catch (Exception ex)
            {
                ErrorService.LogError(SeverityLevel.Error, ErrorType.Database, $"Class {typeof(DataLogger)} failed when attempting to retrieve a roast log from {ROAST_LOG_DATABASE_TABLE_NAME} with ID {roastId}.\n    Log file database location:\n       {configuration.LogFileDatabaseDirectory}", ex);
                return null;
            }
        }
    }
}