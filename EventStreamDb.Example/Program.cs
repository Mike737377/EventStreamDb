using System;
using EventStreamDb.Persistance;
using System.Collections.Generic;
using System.Linq;

namespace EventStreamDb.Example
{
    class Program
    {
        private static readonly InMemoryPersistanceStore _store = new InMemoryPersistanceStore();

        public static class CurrentTime
        {

            private static DateTime _currentTime = DateTime.UtcNow;

            public static DateTime GetTime()
            {
                _currentTime = _currentTime.AddMinutes(1);
                return _currentTime;
            }

        }

        static void Main(string[] args)
        {
            _eventStream = new EventStream(config => config
                .WithCurrentTime(CurrentTime.GetTime)
                .WithPersistantStore(_store)
                .ScanAssemblyWithType<Program>());

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

            Console.WriteLine("Done receiving events");

            var eventsStored = _store.GetEvents();
            Console.WriteLine($"{eventsStored.Count()} events written to persistance store");
        }

        private static UptimeReporter _uptimeReporter = new UptimeReporter();
        private static EventStream _eventStream;

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

            public void Received(SensorDataReceived data, EventMetaData metaData)
            {
                _temperatures.Add(data.Temperature);
                Console.WriteLine($"Temp: {data.Temperature} (avg: {_temperatures.Average().ToString("0.00")})");
            }
        }
    }
}