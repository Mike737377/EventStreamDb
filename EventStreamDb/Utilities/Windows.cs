using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace EventStreamDb
{

    public class RealTimeWindow<T>
    {
        private readonly Timer _timer;
        protected readonly Queue<TimeSeriesData> _seriesData = new Queue<TimeSeriesData>();
        protected readonly object _dataLock = new object();
        private readonly Func<DateTime> _currentTimeResolver;
        public TimeSpan DataValidityPeriod { get; }

        public RealTimeWindow(Func<DateTime> currentTimeResolver, TimeSpan dataValidityPeriod)
        {
            _currentTimeResolver = currentTimeResolver;
            DataValidityPeriod = dataValidityPeriod;

            _timer = new Timer(TimerCallbackHandler, null, Timeout.Infinite, Timeout.Infinite);
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

        private void TimerCallbackHandler(object data)
        {
            RemoveExpiredItems();

            lock (_dataLock)
            {
                if (_seriesData.Count > 0)
                {
                    var waitTime = (_seriesData.Peek().TimeStamp + DataValidityPeriod) - _currentTimeResolver();
                    _timer.Change(waitTime, TimeSpan.FromMilliseconds(-1));
                }
            }
        }

        public void Add(DateTime timeStamp, T data)
        {
            lock (_dataLock)
            {
                _seriesData.Enqueue(new TimeSeriesData(timeStamp, data));
            }

            DataUpdated(timeStamp);
        }

        // public TimeSeriesData[] GetValues()
        // {
        //     return _seriesData.ToArray();
        // }

        protected void DataUpdated(DateTime latestTimeStamp)
        {
            TimerCallbackHandler(null);
        }

        protected void RemoveExpiredItems()
        {
            if (DataValidityPeriod != TimeSpan.Zero)
            {
                lock (_dataLock)
                {
                    while (_seriesData.Count > 0 && HasDataExpired(_seriesData.Peek()))
                    {
                        _seriesData.Dequeue();
                    }
                }
            }
        }

        protected bool HasDataExpired(TimeSeriesData data)
        {
            return data.TimeStamp < (_currentTimeResolver() - DataValidityPeriod);
        }
    }

    public class ReplayWindow<T>
    {
        private readonly Queue<TimeSeriesData> _seriesData = new Queue<TimeSeriesData>();
        protected readonly object _dataLock = new object();
        protected DateTime CurrentTimeStamp { get; private set; } = DateTime.MinValue;
        protected TimeSpan DataValidityPeriod { get; }

        public ReplayWindow(TimeSpan dataValidityPeriod)
        {
            DataValidityPeriod = dataValidityPeriod;
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
            DateTime latestTimeStamp = DateTime.MinValue;

            lock (_dataLock)
            {
                _seriesData.Enqueue(new TimeSeriesData(timeStamp, data));
            }

            CurrentTimeStamp = latestTimeStamp;
            DataUpdated(latestTimeStamp);
        }

        protected void DataUpdated(DateTime latestTimeStamp)
        {
            RemoveExpiredItems();
        }

        protected void RemoveExpiredItems()
        {
            if (DataValidityPeriod != TimeSpan.Zero)
            {
                lock (_dataLock)
                {
                    while (_seriesData.Count > 0 && HasDataExpired(_seriesData.Peek()))
                    {
                        _seriesData.Dequeue();
                    }
                }
            }
        }

        protected bool HasDataExpired(TimeSeriesData data)
        {
            return data.TimeStamp < (CurrentTimeStamp - DataValidityPeriod);
        }

        public RealTimeWindow<T> BecomeRealTimeWindow(Func<DateTime> currentTimeResolver)
        {
            return new RealTimeWindow<T>(currentTimeResolver, DataValidityPeriod);
        }
    }
}