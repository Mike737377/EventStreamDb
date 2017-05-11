namespace EventStreamDb.Persistance
{
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