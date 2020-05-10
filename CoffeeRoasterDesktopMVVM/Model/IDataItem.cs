using System;
using System.Collections.Generic;
using System.Text;

namespace CoffeeRoasterDesktopUI.Model
{
    internal interface IDataItem
    {
        DateTime InitialisedTimeStamp { get; }
        DateTime LastUpdateTimeStamp { get; }
        string Data { get; }

        string LastUpdate();

        void UpdateData(string data);
    }
}