namespace CoffeeRoasterDesktopBackgroundLibrary.Data
{
    using System;

    public class Roast
    {
        public Guid RoastId { get; set; } = new Guid();
        public DateTime Date { get; set; } = new DateTime().Date;
    }
}