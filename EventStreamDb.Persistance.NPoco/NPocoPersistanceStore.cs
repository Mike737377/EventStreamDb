using System;

namespace EventStreamDb.Persistance.NPoco
{
    public class NPocoPersistanceStore : IEventPersistanceStore
    {
        public void StoreEvent<T>(T @event, EventMetaData metaData)
        {
            throw new NotImplementedException();
        }

        public void BeginTransaction()
        {
            throw new NotImplementedException();
        }
        
        public void Rollback()
        {
            throw new NotImplementedException();
        }
        
        public void Commit()        
        {
            throw new NotImplementedException();
        }
    }
}
