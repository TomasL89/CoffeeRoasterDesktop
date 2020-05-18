using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace CoffeeRoasterDesktopBackgroundLibrary.Data
{
    public class Image : INotifyPropertyChanged
    {
        public Guid RoastId { get; set; }
        public Guid Id { get; } = Guid.NewGuid();
        public string ImageName { get; set; }

        public event PropertyChangedEventHandler PropertyChanged;
    }
}