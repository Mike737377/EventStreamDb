using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using EventStreamDb.Persistance;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;

namespace EventStreamDb
{
    public interface IServiceFactory
    {
        object GetInstance(Type type);
    }

    public class DumbServiceFactory : IServiceFactory
    {
        private readonly Dictionary<Type, object> _register = new Dictionary<Type, object>();

        public void Register(Type type, object instance)
        {
            if (_register.ContainsKey(type))
            {

                _register.Add(type, instance);
                return;
            }

            _register[type] = instance;
        }

        public object GetInstance(Type type)
        {
            return Activator.CreateInstance(type);
        }
    }

    public interface IConfig
    {
        DateTime GetCurrentTimeStamp(object @event);
        IEventPersistanceStore GetPersistanceStore();
        IServiceProvider ServiceFactory { get; }
        ProcessHooks Hooks { get; }
        ILoggerFactory GetLoggerFactory();
    }

    public class EventStreamBuilder
    {
        private readonly IConfig _config;

        private EventStreamBuilder(IConfig config)
        {
            _config = config;
        }

        public IEventStream Build()
        {
            return new EventStream(_config);
        }

        public IEventStreamProcessor BuildWithProcessor(TimeSpan processingWindow)
        {
            return BuildWithProcessor(processingWindow, TimeSpan.Zero);
        }

        public IEventStreamProcessor BuildWithProcessor(TimeSpan processingWindow, TimeSpan recieverLag)
        {
            return new EventStreamProcessor(Build(), processingWindow, recieverLag, _config);
        }


        public static EventStreamBuilder Configure(Action<EventStreamConfigBuilder> configurationCallback)
        {
            var configBuilder = new EventStreamConfigBuilder();
            configurationCallback(configBuilder);
            var config = configBuilder.BuildConfig();
            return new EventStreamBuilder(config);
        }
    }


    public class EventStreamConfigBuilder
    {
        private IEventPersistanceStore _store;
        private List<Assembly> _assembliesToScan = new List<Assembly>();
        private IServiceProvider _serviceFactory;
        private Func<object, DateTime> _timestampExtractorCallback;
        private TimeSpan _processingWindowDuration = TimeSpan.Zero;
        private TimeSpan _processingWindowLag = TimeSpan.Zero;

        public EventStreamConfigBuilder WithPersistantStore(IEventPersistanceStore store)
        {
            _store = store;
            return this;
        }

        public EventStreamConfigBuilder WithProcessingWindow(TimeSpan duration, TimeSpan lag)
        {
            _processingWindowDuration = duration;
            _processingWindowLag = lag;
            return this;
        }

        public EventStreamConfigBuilder WithTimeStampExtractor(Func<object, DateTime> getCurrentTimestampCallback)
        {
            _timestampExtractorCallback = getCurrentTimestampCallback;
            return this;
        }

        public EventStreamConfigBuilder WithServiceFactory(IServiceProvider serviceFactory)
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
            return new EventStreamConfig(_store, _serviceFactory, _timestampExtractorCallback, _assembliesToScan);
        }
    }

    public class EventStreamConfig : IConfig
    {
        private readonly IEventPersistanceStore _store;
        private readonly Func<object, DateTime> _currentTimestamp;
        public ProcessHooks Hooks { get; }
        public IServiceProvider ServiceFactory { get; }

        public EventStreamConfig(IEventPersistanceStore store, IServiceProvider serviceFactory, Func<object, DateTime> currentTimestamp, IEnumerable<Assembly> assembliesToScan)
        {
            _store = store;
            _currentTimestamp = currentTimestamp ?? DefaultGetCurrentTimeStamp;
            ServiceFactory = serviceFactory;

            var assembliesToScanList = assembliesToScan?.ToArray();
            var loader = new Loader();
            loader.ScanAssemblies(assembliesToScanList);
            Hooks = loader.GetProcessHooks();
        }

        public DateTime GetCurrentTimeStamp(object @event)
        {
            return _currentTimestamp(@event);
        }

        private DateTime DefaultGetCurrentTimeStamp(object @event)
        {
            var timedEvent = @event as IEvent;

            if (timedEvent == null)
            {
                throw new Exception($@"Cannot get TimeStamp from event type ""{@event.GetType().FullName}"" as it does not implement IEvent. Either implement IEvent or set WithTimeStampExtractor() during event stream configuration");
            }

            return timedEvent.TimeStamp;
        }

        public IEventPersistanceStore GetPersistanceStore()
        {
            return _store;
        }

        public ILoggerFactory GetLoggerFactory()
        {
            return ServiceFactory.GetService<ILoggerFactory>();
        }
    }
}