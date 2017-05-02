using System;
using System.Collections.Generic;
using System.Text;

namespace EventStreamDb.Persistance
{
    public class InMemoryPersistanceStore : IEventPersistanceStore
    {
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
            throw new NotImplementedException();
        }
    }
}
