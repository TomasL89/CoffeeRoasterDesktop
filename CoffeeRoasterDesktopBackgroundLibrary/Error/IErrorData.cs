using System;
using System.Collections.Generic;
using System.Text;

namespace CoffeeRoasterDesktopBackgroundLibrary.Error
{
    internal interface IErrorData
    {
        ErrorType Type { get; set; }
        string Message { get; set; }
    }
}