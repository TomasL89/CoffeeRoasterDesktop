using CoffeeRoasterDesktopBackgroundLibrary;
using CoffeeRoasterDesktopBackgroundLibrary.RoastProfile;
using System.ComponentModel;
using System.Windows.Media;

namespace CoffeeRoasterDesktopUI.ViewModels
{
    public class RoastPointItem : RoastProfilePointBase, INotifyPropertyChanged
    {
        public RoastPoint RoastPoint { get; set; }
        public SolidColorBrush Colour { get; set; }

        public event PropertyChangedEventHandler PropertyChanged;
    }
}