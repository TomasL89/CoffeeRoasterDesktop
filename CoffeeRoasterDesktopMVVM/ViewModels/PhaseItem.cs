using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Media;

namespace CoffeeRoasterDesktopUI.ViewModels
{
    public class PhaseItem
    {
        public SolidColorBrush PhaseColour { get; set; }
        public string PhaseName { get; set; }
        public int PhaseIndex { get; set; }
    }
}
