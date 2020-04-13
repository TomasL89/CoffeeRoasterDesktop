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
        public SolidColorBrush Color { get; set; }

        public event PropertyChangedEventHandler PropertyChanged;

        public void OnRoastPointChanged()
        {
            Console.WriteLine("Changed");
        }
    }
}
