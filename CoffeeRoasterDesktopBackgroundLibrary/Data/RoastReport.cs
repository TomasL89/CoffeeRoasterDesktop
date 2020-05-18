using CoffeeRoasterDesktopBackgroundLibrary.RoastProfile;
using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace CoffeeRoasterDesktopBackgroundLibrary.Data
{
    public class RoastReport : INotifyPropertyChanged
    {
        public Guid Id { get; } = Guid.NewGuid();
        public Guid RoastPlotId { get; set; }
        public string Name { get; set; }
        public DateTime Date { get; } = DateTime.Now.Date;
        public int BatchNumber { get; set; }
        public string RoastType { get; set; }
        public string BeanVariety { get; set; }
        public int BatchWeightRaw { get; set; }
        public int AmbientTemperatureCelcius { get; set; }
        public int HumidityPercentage { get; set; }
        public int RecordedRR { get; set; }
        public int FirstCrackSeconds { get; set; }
        public int RoastTime { get; set; }
        public int BatchWeightRoasted { get; set; }
        public RoastProfile.RoastProfile RoastProfile { get; set; }
        public Guid PhotoOneId { get; set; }
        public Guid PhotoTwoId { get; set; }
        public Guid PhotoThreeId { get; set; }
        public Guid PhotoFourId { get; set; }

        [StringLength(500, ErrorMessage = "The {0} value cannot exceed {1} characters. ")]
        public string Notes { get; set; } = "Enter notes about roast here";

        public event PropertyChangedEventHandler PropertyChanged;
    }
}