using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Media;

namespace CoffeeRoasterDesktopUI.ViewModels
{
    public class PhaseGroupItem : PhaseGroupBase
    {
        public string PhaseName { get; set; }
        public int PhaseStart { get; set; }
        public int PhaseEnd { get; set; }
        public int PhaseRange { get; set; }
        public SolidColorBrush Colour { get; set; }
    }
}
