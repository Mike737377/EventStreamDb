using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace EventStreamDb.Persistance
{
    public class EventMetaData
    {
        public Guid CommitId { get; set; }
        public DateTime TimeStamp { get; set; }
        public string Stream { get; set; }
        public Guid StreamId { get; set; }
        public Type EventType { get; set; }
        public DateTime Watermark { get; set; }
    }
}
