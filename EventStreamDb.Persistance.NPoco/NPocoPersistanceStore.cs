using NPoco;
using System;

namespace EventStreamDb.Persistance.NPoco
{
    public class NPocoPersistanceStore : IEventPersistanceStore
    {
        private readonly IDatabase _database;

        public NPocoPersistanceStore(IDatabase database)
        {
            _database = database;
        }

        public void StoreEvent<T>(T @event, EventMetaData metaData)
        {
            _database.Insert(new StoredEvent(@event, metaData));
        }

        public IEventPersistanceStoreTransaction BeginTransaction()
        {
            return new Transaction(_database, _database.GetTransaction());
        }

        public class Transaction : IEventPersistanceStoreTransaction
        {
            private readonly ITransaction _transaction;
            private readonly IDatabase _database;

            public Transaction(IDatabase database, ITransaction transaction)
            {
                _database = database;
                _transaction = transaction;
            }

            public void StoreEvent<T>(T @event, EventMetaData metaData)
            {
                _database.Insert(new StoredEvent(@event, metaData));
            }

            public void Rollback()
            {
                _transaction.Dispose();
            }

            public void Commit()
            {
                _transaction.Complete();
            }
        }
    }

    public class StoredEvent 
    {
        private StoredEvent() { }
        public StoredEvent(object @event, EventMetaData metaData)
        {
            Event = @event;
            MetaData = metaData;
        }

        public EventMetaData MetaData { get; private set; }
        public object Event { get; private set;}
    }
}
