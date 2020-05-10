﻿// <auto-generated />
namespace Messages
{
    public class TemperatureMessage : IMessage
    {
        public MessageType MessageType { get; set; } = MessageType.TemperatureLog;
        public int Temperature { get; set; }
        public int TimeInSeconds { get; set; }
        public bool HeaterOn { get; set; }
        public double RoastProgress { get; set; }

    }
}
