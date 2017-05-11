using System;
using System.Collections.Generic;
using System.Reflection;
using EventStreamDb.Persistance;

namespace EventStreamDb
{
    public class EventStreamConfigBuilder
    {
        private IEventPersistanceStore _store;
        private List<Assembly> _assembliesToScan = new List<Assembly>();
        private Func<DateTime> _currentTimestamp = () => DateTime.UtcNow;

        public EventStreamConfigBuilder WithPersistantStore(IEventPersistanceStore store)
        {
            _store = store;
            return this;
        }

        public EventStreamConfigBuilder WithCurrentTime(Func<DateTime> getCurrentTimestampCallback)
        {
            _currentTimestamp = getCurrentTimestampCallback;
            return this;
        }

        public EventStreamConfigBuilder ScanAssemblyWithType<T>()
        {
            //_assembliesToScan.Add(typeof(T).Assembly);
            return this;
        }

        internal IConfig BuildConfig()
        {
            return new EventStreamConfig(_store, _currentTimestamp);
        }
    }

    public class EventStreamConfig : IConfig
    {
        private IEventPersistanceStore _store;
        private Func<DateTime> _currentTimestamp;

        public EventStreamConfig(IEventPersistanceStore store, Func<DateTime> currentTimestamp)
        {
            _store = store;
            _currentTimestamp = currentTimestamp;
        }

        public DateTime GetCurrentTimeStamp()
        {
            return _currentTimestamp();
        }

        public IEventPersistanceStore GetPersistanceStore()
        {
            return _store;
        }
    }
}