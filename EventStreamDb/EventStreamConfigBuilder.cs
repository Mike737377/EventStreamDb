using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using EventStreamDb.Persistance;

namespace EventStreamDb
{
    public interface IServiceFactory
    {
        object GetInstance(Type type);
    }

    public class DumbServiceFactory : IServiceFactory
    {
        public object GetInstance(Type type)
        {
            return Activator.CreateInstance(type);
        }
    }

    public interface IConfig
    {
        DateTime GetCurrentTimeStamp();
        IEventPersistanceStore GetPersistanceStore();
        IServiceFactory ServiceFactory { get; }
        ProcessHooks Hooks { get; }
    }

    public class EventStreamConfigBuilder
    {
        private IEventPersistanceStore _store;
        private List<Assembly> _assembliesToScan = new List<Assembly>();
        private Func<DateTime> _currentTimestamp = () => DateTime.UtcNow;
        private IServiceFactory _serviceFactory = new DumbServiceFactory();

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

        public EventStreamConfigBuilder WithServiceFactory(IServiceFactory serviceFactory)
        {
            _serviceFactory = serviceFactory;
            return this;
        }

        public EventStreamConfigBuilder ScanAssemblyWithType<T>()
        {
            _assembliesToScan.Add(typeof(T).GetTypeInfo().Assembly);
            return this;
        }

        public EventStreamConfigBuilder ScanAssembly(Assembly assembly)
        {
            _assembliesToScan.Add(assembly);
            return this;
        }

        public EventStreamConfigBuilder ScanAssemblyWithType(Type typeInAssembly)
        {
            _assembliesToScan.Add(typeInAssembly.GetTypeInfo().Assembly);
            return this;
        }

        internal IConfig BuildConfig()
        {
            return new EventStreamConfig(_store, _serviceFactory, _currentTimestamp, _assembliesToScan);
        }
    }

    public class EventStreamConfig : IConfig
    {
        private readonly IEventPersistanceStore _store;
        private readonly Func<DateTime> _currentTimestamp;
        public ProcessHooks Hooks { get; }
        public IServiceFactory ServiceFactory { get; }

        public EventStreamConfig(IEventPersistanceStore store, IServiceFactory serviceFactory, Func<DateTime> currentTimestamp, IEnumerable<Assembly> assembliesToScan)
        {
            _store = store;
            _currentTimestamp = currentTimestamp;
            ServiceFactory = serviceFactory;

            var assembliesToScanList = assembliesToScan?.ToArray();
            var loader = new Loader();
            loader.ScanAssemblies(assembliesToScanList);
            Hooks = loader.GetProcessHooks();
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