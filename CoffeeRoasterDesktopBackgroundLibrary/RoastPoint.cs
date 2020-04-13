using System.Collections.Specialized;
using System.ComponentModel;

namespace CoffeeRoasterDesktopBackgroundLibrary
{
    public class RoastPoint : INotifyPropertyChanged
    {
        public int StagePoint { get; set; }
        public string StageName { get; set; }
        public int Temperature { get; set; }
        public int StartSeconds { get; set; }
        public int EndSeconds { get; set; }

        public event PropertyChangedEventHandler PropertyChanged;
    }
}