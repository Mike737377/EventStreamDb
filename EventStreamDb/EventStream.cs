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

    public interface IConfig
    {
        DateTime GetCurrentTimeStamp();
        IEventPersistanceStore GetPersistanceStore();
    }

    public class EventStream : IEventStream
    {
        private readonly IConfig _configuration;

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

        public EventStreamTransaction(IConfig configuration)
        {
            _configuration = configuration;
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

            _configuration.GetPersistanceStore().StoreEvent(@event, metaData);

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
            _configuration.GetPersistanceStore().Commit();
        }

        public void Rollback()
        {
            _configuration.GetPersistanceStore().Rollback();
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

    public interface IListen { }

    public interface IListenFor<T> : IListen
    {
        void Received(T data, EventMetaData metaData);
    }

}
