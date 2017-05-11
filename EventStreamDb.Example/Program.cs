using System;
using EventStreamDb.Persistance;
using System.Collections.Generic;
using System.Linq;

namespace EventStreamDb.Example
{
    class Program
    {
        public class EventStreamConfig : IConfig
        {
            private static readonly InMemoryPersistanceStore _store = new InMemoryPersistanceStore();
            public static DateTime CurrentTime = DateTime.UtcNow;

            public DateTime GetCurrentTimeStamp() => IncrementTime();
            public IEventPersistanceStore GetPersistanceStore() => _store;

            private static DateTime IncrementTime()
            {
                CurrentTime = CurrentTime.AddMinutes(1);
                return CurrentTime;
            }
        }

        static void Main(string[] args)
        {
            var config = new EventStreamConfig();
            var eventStream = new EventStream(config);

            eventStream
                .Process(new SensorOnline("CPU"))
                .Process(new SensorDataReceived(45.3m))
                .Process(new SensorDataReceived(42.6m))
                .Process(new SensorDataReceived(40.1m))
                .Process(new SensorDataReceived(40.1m))
                .Process(new SensorDataReceived(40.1m))
                .Process(new SensorDataReceived(39m))
                .Process(new SensorOffline())
                .Process(new SensorOnline("CPU"))
                .Process(new SensorDataReceived(40.5m))
                .Process(new SensorOffline());
        }

        public class AverageTemperature : 
            IListenFor<SensorDataReceived>
        {
            private readonly List<decimal> _temperatures = new List<decimal>();

            public void Received(SensorDataReceived data, EventMetaData metaData)
            {
                _temperatures.Add(data.Temperature);
                Console.WriteLine($"Temp: {data.Temperature} (avg: ${_temperatures.Average()})");
            }
        }
    }
}