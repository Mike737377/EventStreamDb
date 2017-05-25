using System;
using EventStreamDb.Persistance;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace EventStreamDb.Examples.TwitterClone
{
    class Program
    {
        private static readonly InMemoryPersistanceStore _store = new InMemoryPersistanceStore();

        static void Main(string[] args)
        {
            var startTime = new DateTime(1970, 1, 1);
            var partitions = 10;
            var window = TimeSpan.FromSeconds(15);
            var totalSplit = window.TotalSeconds * partitions;

            var nowTime = new DateTime(1969, 12, 31, 23, 59, 49);
            var currentTimeDiff = nowTime - startTime;
            var nowSegment = currentTimeDiff.TotalSeconds / totalSplit;
            var nowSegmentFloor = Math.Floor(nowSegment);
            var startSegmentTime = startTime + TimeSpan.FromSeconds(nowSegmentFloor * totalSplit);
            var remainder = (nowTime - startSegmentTime).TotalSeconds;
            var thePartiton = Math.Floor(remainder / window.TotalSeconds);
            // var thePartiton = remainder % totalSplit;

            Console.WriteLine($"Starts at {startSegmentTime}");
            Console.WriteLine($"Ends at {startSegmentTime.AddSeconds(totalSplit)}");
            Console.WriteLine($"Remainder {remainder}");
            Console.WriteLine($"Partition {thePartiton}");
            






            // _eventStream = new EventStream(config => config
            //     .WithPersistantStore(_store)
            //     .ScanAssemblyWithType<Program>());

            // Console.WriteLine("Streaming events...");

            // _eventStream
            //     .Process(new TweetReceived("samjohnson", Guid.NewGuid(), "hey @danTheMan! You heard the news? #twitterClone"))
            //     .Process(new TweetReceived("danTheMan", Guid.NewGuid(), "@samjohnson Are you talking about #twitterClone?"))
            //     .Process(new TweetReceived("samjohnson", Guid.NewGuid(), "@danTheMan Yep #twitterClone. What do you think of the streaming system?"))
            //     .Process(new TweetReceived("samjohnson", Guid.NewGuid(), "@danTheMan and what's on for the weekend?"))
            //     .Process(new TweetReceived("danTheMan", Guid.NewGuid(), "@samjohnson The #streamingsystem in #twitterClone seems to works well"));

            // Console.WriteLine("Done receiving events");

            // var eventsStored = _store.GetEvents();
            // Console.WriteLine($"{eventsStored.Count()} events written to persistance store");
        }

        private static IEventStream _eventStream;

        public class LinkMentions :
            ITransform<TweetReceived>
        {
            private readonly string _userPattern = "@([a-zA-Z0-9]*)";
            private readonly string _topicPattern = "#([a-zA-Z0-9]*)";

            public void Transform(TweetReceived @event, EventMetaData metaData)
            {
                var usersMentioned = Regex.Matches(@event.Message, _userPattern)
                    .Cast<Match>()
                    .Select(m => new UserMentioned(@event.MessageId, m.Value))
                    .ToArray();

                var topicsMentioned = Regex.Matches(@event.Message, _topicPattern)
                    .Cast<Match>()
                    .Select(m => new TopicMentioned(@event.MessageId, m.Value))
                    .ToArray();

                foreach (var u in usersMentioned) _eventStream.Process(u);
                foreach (var t in topicsMentioned) _eventStream.Process(t);
            }
        }

        public class ConsoleWriter :
            IStore<TweetReceived>,
            IStore<UserMentioned>,
            IStore<TopicMentioned>
        {
            public void Store(TweetReceived @event, EventMetaData metaData) => Console.WriteLine($"Tweet {@event.MessageId}: {@event.Message} - {@event.UserName}");
            public void Store(UserMentioned @event, EventMetaData metaData) => Console.WriteLine($"\tUser mentioned in tweet {@event.MessageId}: {@event.UserName}");
            public void Store(TopicMentioned @event, EventMetaData metaData) => Console.WriteLine($"\tTopic mentioned in tweet {@event.MessageId}: {@event.Topic}");

        }
    }
}