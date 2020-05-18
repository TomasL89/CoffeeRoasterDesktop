using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Media;

namespace CoffeeRoasterDesktopUI.Helpers
{
    public static class ColourHelper
    {
        public static System.Drawing.Color ConvertToColor(SolidColorBrush brush)
        {
            return System.Drawing.Color.FromArgb(brush.Color.A, brush.Color.R, brush.Color.G, brush.Color.B);
        }
    }
}