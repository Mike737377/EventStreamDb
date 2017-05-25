using System;
using System.Collections;
using System.Collections.Generic;
using EventStreamDb.Persistance;
using System.Reflection;
using System.Threading;
using System.Linq;

namespace EventStreamDb
{
    public interface IEventStream
    {
        IEventStream Process<T>(T @event);
        IEventStream Process<T>(IEnumerable<T> @events);

        IEventStream Process<T>(T @event, Action<EventMetaData> eventMetaDataModifier);
        IEventStream Process<T>(IEnumerable<T> @events, Action<EventMetaData> eventMetaDataModifier);
    }

    public class EventStream : IEventStream
    {
        private readonly IConfig _configuration;
        //private readonly IPipeline _pipeline;

        public EventStream(Action<EventStreamConfigBuilder> configure)
        {
            var builder = new EventStreamConfigBuilder();
            configure(builder);

            _configuration = builder.BuildConfig();
        }

        public EventStream(IConfig configuration)
        {
            _configuration = configuration;
        }

        public ITransactionBoundEventStream GetTransaction()
        {
            return new EventStreamTransaction(_configuration);
        }

        public IEventStream Process<T>(T @event)
        {
            GetTransaction().Process(@event).Commit();
            return this;
        }

        public IEventStream Process<T>(IEnumerable<T> @events)
        {
            GetTransaction().Process(@events).Commit();
            return this;
        }

        public IEventStream Process<T>(T @event, Action<EventMetaData> eventMetaDataModifier)
        {
            GetTransaction().Process(@event, eventMetaDataModifier).Commit();
            return this;
        }

        public IEventStream Process<T>(IEnumerable<T> @events, Action<EventMetaData> eventMetaDataModifier)
        {
            GetTransaction().Process(@events, eventMetaDataModifier).Commit();
            return this;
        }
    }

    public interface ITransactionBoundEventStream
    {
        ITransactionBoundEventStream Process<T>(T @event);
        ITransactionBoundEventStream Process<T>(IEnumerable<T> @events);

        ITransactionBoundEventStream Process<T>(T @event, Action<EventMetaData> eventMetaDataModifier);
        ITransactionBoundEventStream Process<T>(IEnumerable<T> @events, Action<EventMetaData> eventMetaDataModifier);

        void Commit();
        void Rollback();
    }

    public class EventStreamTransaction : ITransactionBoundEventStream
    {
        private readonly Guid _commitId = Guid.NewGuid();
        private readonly IConfig _configuration;
        private readonly IEventPersistanceStoreTransaction _transaction;

        public EventStreamTransaction(IConfig configuration)
        {
            _configuration = configuration;
            _transaction = _configuration.GetPersistanceStore().BeginTransaction();
        }

        public ITransactionBoundEventStream Process<T>(T @event)
        {
            Process(@event, null);
            return this;
        }

        public ITransactionBoundEventStream Process<T>(IEnumerable<T> @events)
        {
            Process(@events, null);
            return this;
        }

        public ITransactionBoundEventStream Process<T>(T @event, Action<EventMetaData> eventMetaDataModifier)
        {
            var eventType = @event.GetType();
            var metaData = BuildEventMetaData(@event);
            eventMetaDataModifier?.Invoke(metaData);

            var storeType = typeof(IStore<>).MakeGenericType(eventType).GetTypeInfo();
            var storeMethod = storeType.GetDeclaredMethods("Store").FirstOrDefault(x => x.GetParameters().FirstOrDefault()?.ParameterType == eventType);
            var transformerMethod = eventType.GetTypeInfo().GetDeclaredMethods("Transform").FirstOrDefault(x => x.GetParameters().FirstOrDefault()?.ParameterType == eventType);
            var listenForType = typeof(IListenFor<>).MakeGenericType(eventType).GetTypeInfo();
            var recievedMethod = listenForType.GetDeclaredMethods("Received").FirstOrDefault(x => x.GetParameters().FirstOrDefault()?.ParameterType == eventType);

            _transaction.StoreEvent(@event, metaData);

            var storeInstances = _configuration.Hooks.Stores.ForType(eventType)
                .Select(x => _configuration.ServiceFactory.GetService(x))
                .ToArray();

            foreach (var store in storeInstances)
            {
                storeMethod.Invoke(store, new object[] { @event, metaData });
                //store.Store(@event, metaData);
            }

            var transformerInstances = _configuration.Hooks.Transformers.ForType(eventType)
                .Select(x => _configuration.ServiceFactory.GetService(x))
                .ToArray();

            if (transformerInstances.Any())
            {
                foreach (var transformer in transformerInstances)
                {
                    transformerMethod.Invoke(transformer, new object[] { @event, metaData });
                    //transformer.Transform(@event, metaData);
                }

                return this;
            }

            var listenerInstances = _configuration.Hooks.Listeners.ForType(eventType)
                .Select(x => _configuration.ServiceFactory.GetService(x))
                .ToArray();

            foreach (var listener in listenerInstances)
            {
                recievedMethod.Invoke(listener, new object[] { @event, metaData });
                //listener.Received(@event, metaData);
            }

            return this;
        }

        public ITransactionBoundEventStream Process<T>(IEnumerable<T> @events, Action<EventMetaData> eventMetaDataModifier)
        {
            foreach (var @event in @events)
            {
                Process(@event, eventMetaDataModifier);
            }

            return this;
        }

        private EventMetaData BuildEventMetaData<T>(T @event)
        {
            return new EventMetaData
            {
                CommitId = _commitId,
                EventType = @event.GetType(),
                TimeStamp = _configuration.GetCurrentTimeStamp(@event),
            };
        }

        public void Commit()
        {
            _transaction.Commit();
        }

        public void Rollback()
        {
            _transaction.Rollback();
        }
    }


    public interface IStore<T>
    {
        void Store(T @event, EventMetaData metaData);
    }

    public interface ITransform<T>
    {
        void Transform(T @event, EventMetaData metaData);
    }

    public interface IListenFor<T>
    {
        void Received(T @event, EventMetaData metaData);
    }
}

//public interface IPipeline
//{
//    T Process<T>(T @event, EventMetaData metaData);
//}

//public class ListenerPipeline : IPipeline
//{
//    private readonly IPipeline _innerPipeline;

//    public ListenerPipeline(IPipeline innerPipeline)
//    {
//        _innerPipeline = innerPipeline;
//    }

//    public T Process<T>(T @event, EventMetaData metaData)
//    {
//        return _innerPipeline.Process(@event, metaData);
//    }
//}