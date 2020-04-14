using CoffeeRoasterDesktopBackgroundLibrary;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Runtime.CompilerServices;
using System.Text;
using System.Windows.Media;

namespace CoffeeRoasterDesktopUI.ViewModels
{
    public class RoastPointItem : RoastProfilePointBase, INotifyPropertyChanged
    {
        public new PointType PointType { get; set; } = PointType.ProfilePoint;
        public RoastPoint RoastPoint { get; set; }
        public SolidColorBrush Colour { get; set; }
        public int PhaseGroup { get; set; }

        public event PropertyChangedEventHandler PropertyChanged;
    }
}
