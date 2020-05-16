namespace CoffeeRoasterDesktopBackgroundLibrary.Data
{
    using System;

    public class Roast
    {
        public Guid RoastId { get; set; } = Guid.NewGuid();
        public DateTime Date { get; set; } = DateTime.Now.Date;
    }
}