using System;
using System.Collections.Generic;
using System.Text;

namespace EventStreamDb.Examples.CsvImport
{
    public class SiteVisited
    {
        public SiteVisited(DateTime timestamp, string iPAddress, string userName, string domain)
        {
            Timestamp = timestamp;
            IPAddress = iPAddress;
            UserName = userName;
            Domain = domain;
        }

        public DateTime Timestamp { get;  }
        public string IPAddress { get;  }
        public string UserName { get;  }
        public string Domain { get;  }
    }
}
