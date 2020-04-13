﻿using System;
using System.Collections.Generic;
using System.Text;

namespace Messages
{
    public class TemperatureMessage : IMessage
    {
        public MessageType MessageType { get; set; } = MessageType.TemperatureLog;
        public int Temperature { get; set; }
        public int TimeInSeconds { get; set; }

    }
}
