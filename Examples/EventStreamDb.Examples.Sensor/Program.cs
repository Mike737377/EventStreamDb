using System;
using EventStreamDb.Persistance;
using System.Collections.Generic;
using System.Linq;

namespace EventStreamDb.Examples.Sensor
{
    public class Program
    {
        private static readonly InMemoryPersistanceStore _store = new InMemoryPersistanceStore();
        private static IEventStreamProcessor _eventStream;

        static void Main(string[] args)
        {
            _eventStream = EventStreamBuilder.Configure(c =>
                {
                    c.WithPersistantStore(_store);
                    c.ScanAssemblyWithType<Program>();
                })
                .BuildWithProcessor(TimeSpan.FromMinutes(5));

            Console.WriteLine("Streaming events...");

            _eventStream
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

            _eventStream.Shutdown();

            Console.WriteLine("Done receiving events");

            var eventsStored = _store.GetEvents();
            Console.WriteLine($"{eventsStored.Count()} events written to persistance store");
        }

        private static UptimeReporter _uptimeReporter = new UptimeReporter();

        public class UptimeReporter
        {
            public DateTime OnlineAt { get; private set; } = DateTime.MinValue;
            public DateTime OfflineAt { get; private set; } = DateTime.MinValue;

            public TimeSpan GetUptime()
            {
                return CurrentTime.GetTime() - OnlineAt;
            }

            public void Up(DateTime timeStamp)
            {
                OnlineAt = timeStamp;
            }

            public void Down(DateTime timeStamp)
            {
                OfflineAt = timeStamp;
                Console.WriteLine($"Total uptime: {GetUptime().TotalSeconds} seconds");
            }
        }

        public class FaultAlerter :
            IListenFor<IntermittentSensor>
        {
            public void Received(IntermittentSensor @event, EventMetaData metaData)
            {
                Console.WriteLine($"{@event.Name} sensor is intermittent");
            }
        }

        public class IntermittentSensorReporter :
            IListenFor<SensorOnline>,
            IListenFor<SensorOffline>
        {
            private const int THRESHOLD_IN_SECONDS = 90;

            public void Received(SensorOnline @event, EventMetaData metaData)
            {
                _uptimeReporter.Up(metaData.TimeStamp);

                var timeOffline = _uptimeReporter.OnlineAt.Subtract(_uptimeReporter.OfflineAt).TotalSeconds;

                if (timeOffline < THRESHOLD_IN_SECONDS)
                {
                    _eventStream.Process(new IntermittentSensor(@event.Name));
                }
            }

            public void Received(SensorOffline @event, EventMetaData metaData)
            {
                _uptimeReporter.Down(metaData.TimeStamp);
            }
        }

        public class AverageTemperature :
            IListenFor<SensorDataReceived>
        {
            private static readonly List<decimal> _temperatures = new List<decimal>();

            public void Received(SensorDataReceived @event, EventMetaData metaData)
            {
                _temperatures.Add(@event.Temperature);
                Console.WriteLine($"Temp: {@event.Temperature} (avg: {_temperatures.Average().ToString("0.00")})");
            }
        }
    }
}