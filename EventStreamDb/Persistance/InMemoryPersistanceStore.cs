using System;
using System.Collections.Generic;
using System.Text;

namespace EventStreamDb.Persistance
{
    public class InMemoryPersistanceStore : IEventPersistanceStore
    {
        private List<StoredEvent> _events = new List<StoredEvent>();

        public IEventPersistanceStoreTransaction BeginTransaction()
        {
            return new Transaction(this);
        }

        public IEnumerable<StoredEvent> GetEvents()
        {
           return _events.ToArray();
        }

        //public IEnumerable<EventData> GetEvents(Guid streamId)
        //{
        //    throw new NotImplementedException();
        //}

        public void StoreEvent<T>(T @event, EventMetaData metaData)
        {
            _events.Add(new StoredEvent(@event, metaData));
        }

        public class Transaction : IEventPersistanceStoreTransaction
        {
            private readonly List<StoredEvent> _transactionEvents = new List<StoredEvent>();
            private readonly InMemoryPersistanceStore _store;

            public Transaction(InMemoryPersistanceStore store)
            {
                this._store = store;
            }

            public void StoreEvent<T>(T @event, EventMetaData metaData)
            {
                lock (_transactionEvents)
                {
                    _transactionEvents.Add(new StoredEvent(@event, metaData));
                }
            }

            public void Commit()
            {
                lock (_transactionEvents)
                {
                    lock (_store)
                    {
                        _store._events.AddRange(_transactionEvents);
                        _transactionEvents.Clear();
                    }
                }
            }

            public void Rollback()
            {
                lock (_transactionEvents)
                {
                    _transactionEvents.Clear();
                }
            }
        }

        public class StoredEvent 
        {
            public StoredEvent(dynamic @event, EventMetaData metaData)
            {
                Event = @event;
                MetaData = metaData;
            }

            public EventMetaData MetaData { get; }
            public dynamic Event { get; }
        }
    }
}
