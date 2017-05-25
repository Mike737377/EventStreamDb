using System;
using System.Collections.Generic;
using System.Text;

namespace EventStreamDb
{
    public interface IEvent
    {
        DateTime TimeStamp { get; }
    }
}
