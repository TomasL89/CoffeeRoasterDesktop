using CoffeeRoasterDesktopBackgroundLibrary.Data;

namespace CoffeeRoasterDesktopUI.ViewModels
{
    public class RoastReportViewModel
    {
        public RoastReport RoastReport { get; set; }

        public RoastReportViewModel()
        {
            RoastReport = new RoastReport();
        }
    }
}