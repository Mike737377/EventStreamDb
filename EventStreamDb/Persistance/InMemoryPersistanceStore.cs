using System;
using System.Collections.Generic;
using System.Text;

namespace EventStreamDb.Persistance
{
    public class InMemoryPersistanceStore : IEventPersistanceStore
    {
        private List<StoredEvent> _events = new List<StoredEvent>();
        private List<StoredEvent> _transactionEvents;

        public void BeginTransaction()
        {
            _transactionEvents = new List<StoredEvent>();
        }

        public void Commit()
        {
            _events.AddRange(_transactionEvents);
            _transactionEvents = null;
        }

        public void Rollback()
        {
            _transactionEvents = null;
        }

        //public IEnumerable<EventData> GetEvents()
        //{
        //    throw new NotImplementedException();
        //}

        //public IEnumerable<EventData> GetEvents(Guid streamId)
        //{
        //    throw new NotImplementedException();
        //}

        public void StoreEvent<T>(T @event, EventMetaData metaData)
        {
            _events.Add(new StoredEvent(@event, metaData));
        }
    }
}
