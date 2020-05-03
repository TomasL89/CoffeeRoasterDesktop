using Newtonsoft.Json;
using System.IO;

namespace CoffeeRoasterDesktopBackgroundLibrary
{
    public class ProfileService
    {
        private readonly JsonSerializer jsonSerializer;

        public ProfileService()
        {
            jsonSerializer = new JsonSerializer();
        }

        public void SaveProfile(string fileLocation, RoastProfile roastProfile)
        {
            using var sw = new StreamWriter(fileLocation);
            using var writer = new JsonTextWriter(sw);
            jsonSerializer.Serialize(writer, roastProfile);
        }

        public RoastProfile LoadProfile(string fileLocation)
        {
            var profile = File.ReadAllText(fileLocation);
            return JsonConvert.DeserializeObject<RoastProfile>(profile);
        }

        public string GetProfileAsString(string fileLocation)
        {
            var profile = File.ReadAllText(fileLocation);

            return profile;
        }

        public RoastProfile LoadProfileFromMessage(string message)
        {
            return JsonConvert.DeserializeObject<RoastProfile>(message);
        }
    }
}