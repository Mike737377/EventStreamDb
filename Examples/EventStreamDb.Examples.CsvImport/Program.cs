using System;
using EventStreamDb.Persistance;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;

namespace EventStreamDb.Examples.CsvImport
{
    class Program
    {
        private static readonly InMemoryPersistanceStore _store = new InMemoryPersistanceStore();

        static void Main(string[] args)
        {

            var data = CsvLoader.Load("MOCK_DATA.csv");
            Console.WriteLine(data.Count());

            var serviceProvider = new ServiceCollection()
                .AddLogging()
                .BuildServiceProvider();

            _eventStream = EventStreamBuilder.Configure(config => config
                    .WithPersistantStore(_store)
                    .WithServiceFactory(serviceProvider)
                    .ScanAssemblyWithType<Program>())
                .BuildWithProcessor(TimeSpan.FromMinutes(5), TimeSpan.FromSeconds(30));

            Console.WriteLine("Streaming events...");

            data.Select(x => new SiteVisited(x.Timestamp, x.IPAddress, x.UserName, x.Domain))
                .Select(x => _eventStream.Process(x));

            Console.WriteLine("Done receiving events");

            _eventStream.Shutdown();

            //var eventsStored = _store.GetEvents();
            // Console.WriteLine($"{eventsStored.Count()} events written to persistance store");
        }

        private static IEventStreamProcessor _eventStream;

        // public class LinkMentions :
        //     ITransform<TweetReceived>
        // {
        //     private readonly string _userPattern = "@([a-zA-Z0-9]*)";
        //     private readonly string _topicPattern = "#([a-zA-Z0-9]*)";

        //     public void Transform(TweetReceived @event, EventMetaData metaData)
        //     {
        //         var usersMentioned = Regex.Matches(@event.Message, _userPattern)
        //             .Cast<Match>()
        //             .Select(m => new UserMentioned(@event.MessageId, m.Value))
        //             .ToArray();

        //         var topicsMentioned = Regex.Matches(@event.Message, _topicPattern)
        //             .Cast<Match>()
        //             .Select(m => new TopicMentioned(@event.MessageId, m.Value))
        //             .ToArray();

        //         foreach (var u in usersMentioned) _eventStream.Process(u);
        //         foreach (var t in topicsMentioned) _eventStream.Process(t);
        //     }
        // }

        // public class ConsoleWriter :
        //     IStore<TweetReceived>,
        //     IStore<UserMentioned>,
        //     IStore<TopicMentioned>
        // {
        //     public void Store(TweetReceived @event, EventMetaData metaData) => Console.WriteLine($"Tweet {@event.MessageId}: {@event.Message} - {@event.UserName}");
        //     public void Store(UserMentioned @event, EventMetaData metaData) => Console.WriteLine($"\tUser mentioned in tweet {@event.MessageId}: {@event.UserName}");
        //     public void Store(TopicMentioned @event, EventMetaData metaData) => Console.WriteLine($"\tTopic mentioned in tweet {@event.MessageId}: {@event.Topic}");

        // }
    }
}