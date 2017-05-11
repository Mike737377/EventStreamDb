using System;
using System.Collections.Generic;

namespace EventStreamDb.Persistance
{
    public interface IEventPersistanceStore
    {
        void StoreEvent<T>(T @event, EventMetaData metaData);
        IEventPersistanceStoreTransaction BeginTransaction();
        // IEnumerable<EventData> GetEvents();
        // IEnumerable<EventData> GetEvents(Guid streamId);
    }

    public interface IEventPersistanceStoreTransaction
    {
        void StoreEvent<T>(T @event, EventMetaData metaData);
        void Rollback();
        void Commit();
    }
}