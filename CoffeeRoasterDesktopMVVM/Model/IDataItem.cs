using System;

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