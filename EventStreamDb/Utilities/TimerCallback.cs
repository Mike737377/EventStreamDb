using System;
using System.Threading;

namespace EventStreamDb
{
    // public class TimerCallback
    // {
    //     public void RegisterCallback(Action<>)
    // }

    // public class RealTimeTimer
    // {
    //     private readonly Timer _timer;
    //     private readonly Func<DateTime> _currentTimeResolver;

    //     public RealTimeTimer(Func<DateTime> currentTimeResolver)
    //     {
    //         _timer = new Timer(TimerCallbackHandler, null, Timeout.Infinite, Timeout.Infinite);
    //         _currentTimeResolver = currentTimeResolver;
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

}