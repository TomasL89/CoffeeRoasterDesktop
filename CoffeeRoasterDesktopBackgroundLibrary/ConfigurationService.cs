using CoffeeRoasterDesktopBackgroundLibrary.Error;
using Newtonsoft.Json;
using System;
using System.IO;

namespace CoffeeRoasterDesktopBackground
{
    public class ConfigurationService
    {
        public Configuration SystemConfiguration { get; private set; }

        private readonly string configurationLocation;
        private readonly string directory;
        private readonly JsonSerializer jsonSerializer;

        public ConfigurationService()
        {
            jsonSerializer = new JsonSerializer();
            directory = AppDomain.CurrentDomain.BaseDirectory;
            configurationLocation = Path.Combine(directory, "system.cfg");
            if (!File.Exists(configurationLocation))
            {
                var validConfiguraiton = CreateNewDefaultConfiguration();
                SystemConfiguration = validConfiguraiton ?? throw new Exception("Error with configuration, please log issue");

                return;
            }

            SystemConfiguration = LoadConfiguration();
        }

        public Configuration CreateNewDefaultConfiguration()
        {
            var configuration = new Configuration()
            {
                IpAddress = "192.168.0.0",
                PortNumber = 8180,
                LogFileDatabaseDirectory = Path.Combine(directory, "roast.db"),
                ErrorLogFileLocation = Path.Combine(directory, "log.txt")
            };

            var couldSaveNewConfiguraiton = SaveConfiguration(configuration);

            if (couldSaveNewConfiguraiton)
            {
                return configuration;
            }

            ErrorService.LogError(SeverityLevel.Error, ErrorType.Configuration, $"Class {typeof(ConfigurationService)} failed when attempting to create a configuration at {directory}.\n");

            return null;
        }

        public Configuration LoadConfiguration()
        {
            try
            {
                var configurationFile = File.ReadAllText(configurationLocation);
                SystemConfiguration = JsonConvert.DeserializeObject<Configuration>(configurationFile);
            }
            catch (Exception ex)
            {
                ErrorService.LogError(SeverityLevel.Debug, ErrorType.Configuration, $"Class {typeof(ConfigurationService)} failed when attempting to load the system configuraiton from {configurationLocation}.\n", ex);

                return new Configuration();
            }

            return SystemConfiguration;
        }

        public bool SaveConfiguration(Configuration configuration)
        {
            try
            {
                using var sw = new StreamWriter(configurationLocation);
                using var writer = new JsonTextWriter(sw);
                jsonSerializer.Serialize(writer, configuration);
            }
            catch (Exception ex)
            {
                ErrorService.LogError(SeverityLevel.Error, ErrorType.Configuration, $"Class {typeof(ConfigurationService)} failed when attempting to save the system configuraiton from {configurationLocation}.\n", ex);

                return false;
            }

            return true;
        }

        public bool UpdateConfiguration(string ipaddress, int portNumber)
        {
            SystemConfiguration.IpAddress = ipaddress;
            SystemConfiguration.PortNumber = portNumber;

            return SaveConfiguration(SystemConfiguration);
        }
    }
}