using Newtonsoft.Json;
using System.Collections.Generic;
using System.IO;

namespace CoffeeRoasterDesktopBackgroundLibrary.RoastProfile
{
    public class ProfileService
    {
        private readonly JsonSerializer jsonSerializer;

        public ProfileService()
        {
            jsonSerializer = new JsonSerializer();
        }

        public RoastProfile ValidateRoastProfileAndDecode(string roastProfileFromDevice)
        {
            if (string.IsNullOrWhiteSpace(roastProfileFromDevice))
                return null;

            try
            {
                var roastProfile = JsonConvert.DeserializeObject<RoastProfile>(roastProfileFromDevice);
                // Check for roast points
                if (roastProfile.RoastPoints.Count < 1)
                    return null;

                // Check roast total length
                if (roastProfile.RoastLengthTotalInSeconds <= 0)
                    return null;

                return roastProfile;
            }
            catch (System.Exception ex)
            {
                return null;
            }
        }

        public bool SaveProfile(string fileLocation, RoastProfile roastProfile)
        {
            if (!File.Exists(fileLocation))
                return false;

            using var sw = new StreamWriter(fileLocation);
            using var writer = new JsonTextWriter(sw);
            jsonSerializer.Serialize(writer, roastProfile);

            return true;
        }

        public RoastProfile LoadProfile(string fileLocation)
        {
            if (!File.Exists(fileLocation))
                return null;

            var profile = File.ReadAllText(fileLocation);
            return JsonConvert.DeserializeObject<RoastProfile>(profile);
        }

        public string GetProfileAsString(string fileLocation)
        {
            if (string.IsNullOrEmpty(fileLocation))
                return string.Empty;

            var profile = File.ReadAllText(fileLocation);

            return profile;
        }

        public RoastProfile LoadProfileFromMessage(string message)
        {
            if (string.IsNullOrWhiteSpace(message))
                return null;

            return JsonConvert.DeserializeObject<RoastProfile>(message);
        }

        public RoastProfile RebuildProfile(string profileShell, List<string> profilePoints)
        {
            try
            {
                var newProfile = JsonConvert.DeserializeObject<RoastProfile>(profileShell);

                foreach (var point in profilePoints)
                {
                    var roastPoint = point.Replace(System.Environment.NewLine, string.Empty);
                    newProfile.RoastPoints.Add(JsonConvert.DeserializeObject<RoastPoint>(roastPoint));
                }
                return newProfile;
            }
            catch (System.Exception e)
            {
                return null;
            }
        }
    }
}