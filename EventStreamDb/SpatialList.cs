using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace EventStreamDb
{

    // public abstract class ExpiringTimeSpatialList<T>
    // {
    //     protected Queue<TimeSeriesData> _seriesData = new Queue<TimeSeriesData>();
    //     protected readonly object _dataLock = new object();
    //     private readonly Func<DateTime> _currentTimeResolver;
    //     private readonly TimeSpan _dataValidityPeriod;

    //     public ExpiringTimeSpatialList(Func<DateTime> currentTimeResolver, TimeSpan dataValidityPeriod)
    //     {
    //         _currentTimeResolver = currentTimeResolver;
    //         _dataValidityPeriod = dataValidityPeriod;
    //     }

    //     protected class TimeSeriesData
    //     {
    //         public TimeSeriesData(DateTime timeStamp, T data)
    //         {
    //             TimeStamp = timeStamp;
    //             Data = data;
    //         }
    //         public DateTime TimeStamp { get; }
    //         public T Data { get; }
    //     }

    //     public void Add(DateTime timeStamp, T data)
    //     {
    //         DateTime latestTimeStamp = DateTime.MinValue;

    //         lock (_dataLock)
    //         {
    //             _seriesData.Enqueue(new TimeSeriesData(timeStamp, data));
    //             var orderedData = _seriesData.OrderBy(x => x.TimeStamp).ToArray();
    //             latestTimeStamp = orderedData.LastOrDefault()?.TimeStamp ?? DateTime.MinValue;
    //             _seriesData = new Queue<TimeSeriesData>(orderedData);
    //         }

    //         DataUpdated(latestTimeStamp);
    //     }

    //     protected virtual void DataUpdated(DateTime latestTimeStamp) { }

    //     protected void RemoveExpiredItems()
    //     {
    //         while (_seriesData.Count > 0 && DataHasExpired(_seriesData.Peek()))
    //         {
    //             _seriesData.Dequeue();
    //         }
    //     }

    //     protected bool DataHasExpired(TimeSeriesData data)
    //     {
    //         return data.TimeStamp < (_currentTimeResolver() - _dataValidityPeriod);
    //     }

    // }

    // public class RealtimeExpiringTimeSpatialList<T> : ExpiringTimeSpatialList<T>
    // {
    //     private readonly Timer _timer;

    //     public RealtimeExpiringTimeSpatialList(Func<DateTime> currentTimeResolver, TimeSpan dataValidityPeriod)
    //         : base(currentTimeResolver, dataValidityPeriod)
    //     {
    //         _timer = new Timer(TimerCallbackHandler, null, Timeout.Infinite, Timeout.Infinite);
    //     }

    //     protected override void DataUpdated(DateTime latestTimeStamp)
    //     {
    //         TimerCallbackHandler(null);
    //     }

    //     private void TimerCallbackHandler(object data)
    //     {
    //         lock (_dataLock)
    //         {
    //             RemoveExpiredItems();

    //             if (_seriesData.Count > 0)
    //             {
    //                 var waitTime = _seriesData.Peek().TimeStamp - _currentTimeResolver();
    //                 _timer.Change(waitTime, TimeSpan.FromMilliseconds(-1));
    //             }
    //         }
    //     }
    // }

    // public class LatestExpiringTimeSpatialList<T> : ExpiringTimeSpatialList<T>
    // {
    //     private DateTime _latestTime = DateTime.MinValue;

    //     public LatestExpiringTimeSpatialList(TimeSpan dataValidityPeriod)
    //         : base(() => GetCurrentTime(this), dataValidityPeriod)
    //     {
    //     }

    //     private static DateTime GetCurrentTime(LatestExpiringTimeSpatialList<T> latestExpiringTimeSpatialList)
    //     {
    //         return latestExpiringTimeSpatialList._latestTime;
    //     }

    //     protected override void DataUpdated(DateTime latestTimeStamp)
    //     {
    //         lock (_dataLock)
    //         {
    //             RemoveExpiredItems();
    //             _latestTime = latestTimeStamp;
    //         }
    //     }
    // }
}