using System;
using System.Collections.Generic;
using System.Text;

namespace EventStreamDb.Examples.TwitterClone
{
    public class TweetReceived
    {
        public string UserName { get; }
        public Guid MessageId { get; }
        public string Message { get; }

        public TweetReceived(string userName, Guid messageId, string message)
        {
            UserName = userName;
            MessageId = messageId;
            Message = message;
        }
    }

    public class UserMentioned
    {
        public Guid MessageId { get; }
        public string UserName { get; }

        public UserMentioned(Guid messageId, string userName)
        {
            MessageId = messageId;
            UserName = userName;
        }
    }

    public class TopicMentioned
    {
        public Guid MessageId { get; }
        public string Topic { get; }

        public TopicMentioned(Guid messageId, string topic)
        {
            MessageId = messageId;
            Topic = topic;
        }
    }

}
