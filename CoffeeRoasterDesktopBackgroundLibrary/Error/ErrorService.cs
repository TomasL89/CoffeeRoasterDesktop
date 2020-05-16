using System;
using System.IO;
using System.Reflection;
using Serilog;

namespace CoffeeRoasterDesktopBackgroundLibrary.Error
{
    public static class ErrorService
    {
        private static string logFileLocation = Path.Combine(Assembly.GetEntryAssembly().Location, "log.txt");

        public static void LogError(SeverityLevel severityLevel, ErrorType errorType, string message, Exception exception = null)
        {
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Debug()
                .WriteTo.File(logFileLocation, rollingInterval: RollingInterval.Day)
                .CreateLogger();

            switch (severityLevel)
            {
                case SeverityLevel.Debug:
                    Log.Debug($"{errorType} { message} Exception: {exception}");
                    break;

                case SeverityLevel.Warning:
                    Log.Warning($"{errorType} { message} Exception: {exception}");
                    break;

                case SeverityLevel.Error:
                    Log.Error($"{errorType} { message} Exception: {exception}");
                    break;

                case SeverityLevel.Fatal:
                    Log.Fatal($"{errorType} { message} Exception: {exception}");
                    break;
            }
        }
    }
}