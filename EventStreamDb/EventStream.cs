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

        public void Commit()
        {
            throw new NotImplementedException();
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

            Process(@event, metaData);

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

        public ITransactionBoundEventStream Process<T>(IEnumerable<T> @events, Action<EventMetaData> eventMetaDataModifier)
        {
            foreach (var @event in @events)
            {
                Process(@event, eventMetaDataModifier);
            }

            return this;
        }

        public void Rollback()
        {
            throw new NotImplementedException();
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
        void Recieved(T data, EventMetaData metaData);
    }


    public class TimeSpatialList<T>
    {
        private Queue<TimeSeriesData> _data = new Queue<TimeSeriesData>();
        private readonly Func<DateTime> _currentTimeResolver;
        private readonly Timer _timer;
        private readonly object _dataLock = new object();
        private readonly TimeSpan _dataValidityPeriod;

        public TimeSpatialList(Func<DateTime> currentTimeResolver, TimeSpan dataValidityPeriod)
        {
            _currentTimeResolver = currentTimeResolver;
            _dataValidityPeriod = dataValidityPeriod;

            _timer = new Timer(TimerCallbackHandler, null, Timeout.Infinite, Timeout.Infinite);
        }

        private class TimeSeriesData
        {
            public TimeSeriesData(DateTime timeStamp, T data)
            {
                TimeStamp = timeStamp;
                Data = data;
            }

            public DateTime TimeStamp { get; }
            public T Data { get; }
        }

        public void Add(DateTime timeStamp, T data)
        {
            lock (_dataLock)
            {
                _data.Enqueue(new TimeSeriesData(timeStamp, data));
                _data = new Queue<TimeSeriesData>(_data.ToArray().OrderBy(x => x.TimeStamp));
            }

            TimerCallbackHandler(null);
        }

        private void TimerCallbackHandler(object data)
        {
            lock (_dataLock)
            {
                RemoveExpiredItems();

                if (_data.Count > 0)
                {
                    var waitTime = _data.Peek().TimeStamp - _currentTimeResolver();
                    _timer.Change(waitTime, TimeSpan.FromMilliseconds(-1));
                }
            }
        }

        private void RemoveExpiredItems()
        {
            while (_data.Count > 0 && DataHasExpired(_data.Peek()))
            {
                _data.Dequeue();
            }
        }

        private bool DataHasExpired(TimeSeriesData data)
        {
            return data.TimeStamp < (_currentTimeResolver() - _dataValidityPeriod);
        }

    }

}
