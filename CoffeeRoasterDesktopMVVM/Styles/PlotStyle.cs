using System.Windows.Media;

namespace CoffeeRoasterDesktopUI.Styles
{
    // Colormind Reference
    public static class PlotStyle
    {
        public static SolidColorBrush ColormindWhite { get; } = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#F6F4F0"));
        public static SolidColorBrush ColormindOrange { get; } = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#C69358"));
        public static SolidColorBrush ColormindGrey { get; } = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#B7ACB3"));
        public static SolidColorBrush ColormindRed { get; } = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#CE5551"));
        public static SolidColorBrush ColormindGreen { get; } = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#315E4F"));
    }
}