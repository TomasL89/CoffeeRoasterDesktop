﻿using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

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
    }
}
