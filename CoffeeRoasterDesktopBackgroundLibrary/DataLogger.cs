namespace CoffeeRoasterDesktopBackgroundLibrary
{
    using CoffeeRoasterDesktopBackground;
    using CoffeeRoasterDesktopBackgroundLibrary.Data;
    using LiteDB;
    using System;
    using System.Diagnostics;

    public class DataLogger
    {
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
                using (var db = new LiteDatabase(configuration.LogFileDatabaseDirectory))
                {
                    var roast = new Roast();
                    var roastTable = db.GetCollection<Roast>("roast");
                    roastTable.Insert(new Roast());
                    return roast.RoastId;
                }
            }
            catch (Exception)
            {
                return Guid.Empty;
            }
        }

        public void SaveToDatabase(RoastLog roastLogRow)
        {
            using var db = new LiteDatabase(configuration.LogFileDatabaseDirectory);
            var roastLogs = db.GetCollection<RoastLog>("roastlogs");

            roastLogs.Insert(roastLogRow);
        }

        public void GetRoastById(Guid roastId)
        {
            using var db = new LiteDatabase(configuration.LogFileDatabaseDirectory);
            var roastLog = db.GetCollection<RoastLog>("roastlogs");

            var results = roastLog.Query().Where(x => x.RoastId == roastId).ToList();

            Debug.WriteLine("TimeStamp _ Temperature _ Roast Time(s) _Heater On");
            foreach (var row in results)
            {
                // todo delete once done with testing
                Debug.WriteLine($"{row.TimeStamp} {row.Temperature} _ {row.RoastDurationSecond} _ {row.HeaterOn}");
            }
        }
    }
}