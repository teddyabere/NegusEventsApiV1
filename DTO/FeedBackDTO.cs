using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;

namespace NegusEventsApi.DTO
{
    public class FeedBackDTO
    {
        public string Id { get; set; }

        public string EventId { get; set; }

        public string AttendeeId { get; set; }

        public int Rating { get; set; }

        public string Comment { get; set; }

    }
}
