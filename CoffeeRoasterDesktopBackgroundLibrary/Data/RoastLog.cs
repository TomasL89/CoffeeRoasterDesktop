namespace CoffeeRoasterDesktopBackgroundLibrary.Data
{
    using System;

    public class RoastLog
    {
        public Guid RoastId { get; set; }
        public DateTime TimeStamp { get; set; }
        public int RoastDurationSecond { get; set; }
        public double Temperature { get; set; }
        public bool HeaterOn { get; set; }
    }
}