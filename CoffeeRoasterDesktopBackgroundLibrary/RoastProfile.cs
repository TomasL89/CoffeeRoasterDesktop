using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace CoffeeRoasterDesktopBackgroundLibrary
{
    public class RoastProfile
    {
        public string RoastName { get; set; }
        public int RoastLengthTotalInSeconds { get; set; }
        public List<RoastPoint> RoastPoints { get; set; }
    }
}
