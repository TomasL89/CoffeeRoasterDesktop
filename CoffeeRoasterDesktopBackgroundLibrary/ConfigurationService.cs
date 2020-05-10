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

        //todo system configuration changed event

        public ConfigurationService()
        {
            jsonSerializer = new JsonSerializer();
            directory = AppDomain.CurrentDomain.BaseDirectory;
            configurationLocation = Path.Combine(directory, "system.cfg");
            if (!File.Exists(configurationLocation))
            {
                // warn user to create a new configuration??
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
                LogFileDatabaseDirectory = Path.Combine(directory, "roast.db")
            };

            var couldSaveNewConfiguraiton = SaveConfiguration(configuration);

            if (couldSaveNewConfiguraiton)
            {
                return configuration;
            }

            return null;
        }

        public Configuration LoadConfiguration()
        {
            try
            {
                var configurationFile = File.ReadAllText(configurationLocation);
                SystemConfiguration = JsonConvert.DeserializeObject<Configuration>(configurationFile);
            }
            catch (Exception)
            {
                // todo see how well this works
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
            catch (Exception)
            {
                // todo handle exceptions here, display a warning message
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