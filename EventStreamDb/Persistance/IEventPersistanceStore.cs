using System;
using System.Collections.Generic;

namespace EventStreamDb.Persistance
{
    public interface IEventPersistanceStore
    {
        void StoreEvent<T>(T @event, EventMetaData metaData);
        void BeginTransaction();
        void Rollback();
        void Commit();

        //IEnumerable<EventData> GetEvents();
        //IEnumerable<EventData> GetEvents(Guid streamId);
    }
}