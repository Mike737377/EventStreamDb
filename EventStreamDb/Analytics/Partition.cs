using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace EventStreamDb.Analytics
{
    public interface IPartition
    {
        IEnumerable<object> Events { get; }
    }

    public class Partition : IPartition
    {
        public readonly List<object> Events = new List<object>();

        IEnumerable<object> IPartition.Events => Events;
    }

    public interface IPartitionProcessor
    {
        void Process(Partition partition);
    }

    public class PartitionManager
    {
        private readonly TimeSpan partitionWindow;
        private readonly int partitions;
        private readonly Partition[] q;
        private readonly object partitionLock = new object();

        public PartitionManager(TimeSpan partitionWindow, int partitions, IPartitionProcessor partitionProcessor)
        {
            this.partitions = partitions;
            this.partitionWindow = partitionWindow;
            this.q = Enumerable.Range(0, partitions).Select(x => new Partition()).ToArray();
        }

        private void ProcessPartition()
        {
            IPartition dequeuedPartition = null;

            lock (partitionLock)
            {
                //dequeuedPartition = this.q[]

            }

            if (dequeuedPartition != null)
            {
                
            }
        }

        private int GetPartition(DateTime timestamp)
        {
            throw new NotImplementedException();
            // timestamp
        }

        public void AddEvent(object @event)
        {
            
        }
    }
}