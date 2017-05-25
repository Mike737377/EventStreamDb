using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace EventStreamDb
{
    public interface IEventStreamProcessor : IDisposable
    {
        IEventStreamProcessor Process<T>(T @event);
        IEventStreamProcessor Process<T>(params T[] @events);
        void Shutdown();
    }

    public class EventStreamProcessor : IEventStreamProcessor
    {
        private readonly ProcessingWindow _processingWindow;
        private readonly IEventStream _eventStream;
        private readonly IConfig _config;
        private readonly ILogger _logger;

        public EventStreamProcessor(IEventStream eventStream, TimeSpan processingWindow, TimeSpan processingLag, IConfig config)
        {
            _eventStream = eventStream;
            _config = config;
            _logger = config.GetLoggerFactory().CreateLogger<EventStreamProcessor>();

            if (processingWindow != TimeSpan.Zero)
            {
                _processingWindow = new ProcessingWindow(eventStream, processingWindow, processingLag, _logger);
            }
        }

        public void Dispose()
        {
            Shutdown();
        }

        public IEventStreamProcessor Process<T>(params T[] events)
        {
            foreach (var item in events)
            {
                Process(item);
            }

            return this;
        }

        public IEventStreamProcessor Process<T>(T @event)
        {
            if (_processingWindow == null)
            {
                _eventStream.Process(@event);
            }
            else
            {
                _processingWindow.Add(_config.GetCurrentTimeStamp(@event), @event);
            }

            return this;
        }

        public void Shutdown()
        {
            if (_processingWindow != null)
            {
                lock (_processingWindow)
                {
                    _processingWindow.ForceTumble();
                }
            }
        }

        private class ProcessingWindow : TumblingWindow<dynamic>
        {
            private readonly IEventStream _eventStream;
            private readonly ILogger _logger;

            public ProcessingWindow(IEventStream eventStream, TimeSpan duration, TimeSpan lag, ILogger logger)
                : base(duration, lag)
            {
                _eventStream = eventStream;
                _logger = logger;
            }

            protected override void Tumbling(TumbledSet tumbled)
            {
                var events = tumbled.Data.ToArray();
                _logger.LogTrace($"Processing {events.Length} events from {tumbled.EntryTime} to {tumbled.ExitTime}");
                _eventStream.Process(events.AsEnumerable(), x => x.Watermark = tumbled.ExitTime);
            }
        }
    }

}
