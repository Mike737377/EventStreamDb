using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace EventStreamDb
{
    public class TumblingWindow<T>
    {
        public DateTime CurrentTimeStamp { get; private set; } = DateTime.MinValue;
        public TimeSpan Duration { get; }
        public TimeSpan Lag { get; }

        private List<T> _currentWindow = null;
        private List<T> _futureWindow = new List<T>();
        private DateTime _windowEntry = DateTime.MinValue;
        private DateTime _windowExit;

        private readonly object _dataLock = new object();
        private readonly TimeSpan _exclusiveDuration;

        protected virtual void Tumbling(TumbledSet tumbled) { }

        private const int MINIMUM_DURATION_IN_SECONDS = 1;

        public TumblingWindow(TimeSpan duration)
            : this(duration, TimeSpan.Zero)
        { }

        public TumblingWindow(TimeSpan duration, TimeSpan lag)
        {
            if (duration.TotalSeconds < MINIMUM_DURATION_IN_SECONDS)
            {
                throw new Exception($"Minimum tumbling window duration is {MINIMUM_DURATION_IN_SECONDS} second");
            }

            Duration = duration.Duration();
            Lag = lag.Duration();

            _exclusiveDuration = duration.Subtract(TimeSpan.FromMilliseconds(1));
        }

        public class TumbledSet
        {
            public TumbledSet(DateTime entryTime, DateTime exitTime, T[] data)
            {
                EntryTime = entryTime;
                ExitTime = exitTime;
                Data = data;
            }

            public DateTime EntryTime { get; }
            public DateTime ExitTime { get; }
            public T[] Data { get; }
        }

        public void Add(DateTime timeStamp, T data)
        {
            lock (_dataLock)
            {
                if (_currentWindow == null)
                {
                    InitNextWindow(FindInitialWindowEntryTime(timeStamp));
                }

                while (IsReadyToTumble(timeStamp))
                {
                    var tumbled = ForceTumble();
                    InitNextWindow(tumbled.EntryTime.Add(Duration));
                }

                var windowToUse = timeStamp > _windowExit ? _futureWindow : _currentWindow;
                windowToUse.Add(data);

                CurrentTimeStamp = timeStamp;
            }
        }

        private DateTime FindInitialWindowEntryTime(DateTime timeStamp)
        {
            var remainder = (timeStamp - new DateTime(2000, 1, 1)).TotalSeconds % Duration.TotalSeconds;
            return timeStamp.Add(TimeSpan.FromSeconds(Math.Abs(remainder)));
        }

        public TumbledSet ForceTumble()
        {
            var tumbled = new TumbledSet(_windowEntry, _windowExit, _currentWindow.ToArray());
            _currentWindow.Clear();

            Tumbling(tumbled);

            return tumbled;
        }

        private void InitNextWindow(DateTime entryTime)
        {
            _currentWindow = _futureWindow;
            _futureWindow = new List<T>();
            _windowEntry = entryTime;
            _windowExit = entryTime.Add(_exclusiveDuration);
        }

        private bool IsReadyToTumble(DateTime timeStamp)
        {
            return timeStamp > (_windowExit + Lag);
        }
    }

    public class SlidingWindow<T>
    {
        private readonly Queue<TimeSeriesData> _seriesData = new Queue<TimeSeriesData>();
        private readonly object _dataLock = new object();
        public DateTime CurrentTimeStamp { get; private set; } = DateTime.MinValue;
        public TimeSpan Duration { get; }

        protected IEnumerable<TimeSeriesData> CurrentData { get => _seriesData.ToArray(); }
        protected virtual void DataAdded(TimeSeriesData dataAdded) { }
        protected virtual void DataExpired(TimeSeriesData dataExpired) { }
        protected virtual void DiscardedEntry(TimeSeriesData dataExpired) { }

        public SlidingWindow(TimeSpan duration)
        {
            Duration = duration;
        }

        protected class TimeSeriesData
        {
            public TimeSeriesData(DateTime timeStamp, T data)
            {
                TimeStamp = timeStamp;
                Data = data;
            }

            public DateTime TimeStamp { get; }
            public T Data { get; }
        }

        public void Add(DateTime timeStamp, T data)
        {
            var seriesData = new TimeSeriesData(timeStamp, data);

            lock (_dataLock)
            {
                if (timeStamp < CurrentTimeStamp)
                {
                    DiscardedEntry(seriesData);
                }
                else
                {
                    _seriesData.Enqueue(seriesData);
                    CurrentTimeStamp = seriesData.TimeStamp;
                }
            }

            DataAdded(seriesData);
            RemoveExpiredItems();
        }

        protected void RemoveExpiredItems()
        {
            if (Duration != TimeSpan.Zero)
            {
                lock (_dataLock)
                {
                    while (_seriesData.Count > 0 && HasDataExpired(_seriesData.Peek()))
                    {
                        DataExpired(_seriesData.Dequeue());
                    }
                }
            }
        }

        protected bool HasDataExpired(TimeSeriesData data)
        {
            return data.TimeStamp < (CurrentTimeStamp - Duration);
        }
    }
}