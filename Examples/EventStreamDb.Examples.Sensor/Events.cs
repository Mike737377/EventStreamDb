using System;
using System.Collections.Generic;
using System.Text;

namespace EventStreamDb.Examples.Sensor
{
    public abstract class Event : IEvent
    {
        public DateTime TimeStamp { get; } = CurrentTime.GetTime();
    }

    public class SensorOnline : Event
    {
        public string Name { get; }

        public SensorOnline(string name)
        {
            Name = name;
        }
    }

    public class SensorDataReceived : Event
    {
        public decimal Temperature { get; }

        public SensorDataReceived(decimal temperature)
        {
            Temperature = temperature;
        }
    }

    public class SensorOffline : Event { }

    public class IntermittentSensor : Event
    {
        public string Name { get; }

        public IntermittentSensor(string name)
        {
            Name = name;
        }
    }
}
