using System;
using System.Collections;
using System.Collections.Generic;
using EventStreamDb.Persistance;
using System.Threading;
using System.Linq;

namespace EventStreamDb
{
    public interface IEventStream
    {
        IEventStream Process<T>(T @event);
        IEventStream Process<T>(IEnumerable<T> @event);

        IEventStream Process<T>(T @event, Action<EventMetaData> eventMetaDataModifier);
        IEventStream Process<T>(IEnumerable<T> @event, Action<EventMetaData> eventMetaDataModifier);
    }

    public class EventStream : IEventStream
    {
        private readonly IConfig _configuration;
        private readonly IPipeline _pipeline;

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

        public IEventStream Process<T>(IEnumerable<T> @event)
        {
            GetTransaction().Process(@event).Commit();
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
            var metaData = BuildEventMetaData(@event);
            eventMetaDataModifier?.Invoke(metaData);

            _transaction.StoreEvent(@event, metaData);

            var listenerInstances = _configuration.Hooks.Listeners.ForType<T>()
                .Select(x => _configuration.ServiceFactory.GetInstance(x) as IListenFor<T>)
                .ToArray();

            foreach (var listener in listenerInstances)
            {
                listener.Received(@event, metaData);
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
                TimeStamp = _configuration.GetCurrentTimeStamp(),
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


    public interface ICanStore { }

    public interface IStore<T> : ICanStore
    {
        void Store(T data, EventMetaData metaData);
    }

    public interface ICanTransform { }

    public interface ITransform<TIn, TOut> : ICanTransform
    {
        TOut Transform(TIn source);
    }

    public interface IListenFor<T> 
    {
        void Received(T data, EventMetaData metaData);
    }
}

public interface IPipeline
{
    T Process<T>(T @event, EventMetaData metaData);
}

public class ListenerPipeline : IPipeline
{
    private readonly IPipeline _innerPipeline;

    public ListenerPipeline(IPipeline innerPipeline)
    {
        _innerPipeline = innerPipeline;
    }

    public T Process<T>(T @event, EventMetaData metaData)
    {
        return _innerPipeline.Process(@event, metaData);
    }
}