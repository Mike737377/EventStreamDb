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

    public class SensorDataRecieved
    {
        public decimal Temperature { get; }

        public SensorDataRecieved(decimal temperature)
        {
            Temperature = temperature;
        }
    }

    public class SensorOffline { }
}
