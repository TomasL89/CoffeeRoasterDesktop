using System;

namespace CoffeeRoasterDesktopUI.Model
{
    public class RoasterWiFiData : IDataItem
    {
        public DateTime InitialisedTimeStamp { get; private set; }

        public DateTime LastUpdateTimeStamp { get; private set; }

        public string Data { get; private set; }
        public string LastUpdated { get; set; }

        public RoasterWiFiData()
        {
            InitialisedTimeStamp = DateTime.Now;
        }

        public string LastUpdate()
        {
            return $"{Data} %";
        }

        public void UpdateData(string data)
        {
            Data = data;
        }
    }
}