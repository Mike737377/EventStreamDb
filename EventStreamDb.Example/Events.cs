using System;
using System.Collections.Generic;
using System.Text;

namespace EventStreamDb.Example
{
    public class SensorOnline
    {
        public string Name { get; }

        public SensorOnline(string name)
        {
            Name = name;
        }
    }

    public class SensorDataReceived
    {
        public decimal Temperature { get; }

        public SensorDataReceived(decimal temperature)
        {
            Temperature = temperature;
        }
    }

    public class SensorOffline { }
}
