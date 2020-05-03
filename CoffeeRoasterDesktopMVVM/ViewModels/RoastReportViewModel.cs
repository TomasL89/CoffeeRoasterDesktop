using CoffeeRoasterDesktopBackgroundLibrary.Data;
using System;
using System.Collections.Generic;
using System.Text;

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