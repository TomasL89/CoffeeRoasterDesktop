using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Input;

namespace CoffeeRoasterDesktopUI.ViewModels
{
    public class RoastPointButton : RoastProfilePointBase
    {
        public new PointType PointType { get; set; } = PointType.Button;
        public string Name { get; set; } = "AddRoastPoint";
    }
}
