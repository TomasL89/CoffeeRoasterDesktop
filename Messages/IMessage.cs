using System;
using System.Collections.Generic;
using System.Text;

namespace Messages
{
    public interface IMessage
    {
        MessageType MessageType { get; set; }
    }
}
