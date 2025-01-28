using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;
using System;

namespace NegusEventsApi.Models.Feedback
{
    public class Feedbacks
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string? Id { get; set; }

        [BsonElement("event")]
        public EventInfo Event { get; set; }

        [BsonElement("attendee")]
        public AttendeeInfo Attendee { get; set; }

        [BsonElement("rating")]
        public int Rating { get; set; }

        [BsonElement("comment")]
        public string Comment { get; set; }

        [BsonElement("createdAt")]
        public string CreatedAt { get; set; }

        [BsonElement("updatedAt")]
        public string UpdatedAt { get; set; }
    }

    public class EventInfo
    {
        [BsonElement("eventId")]
        public string EventId { get; set; }

        [BsonElement("createdAt")]
        public string CreatedAt { get; set; }

        [BsonElement("organizerId")]
        public string OrganizerId { get; set; }
    }

    public class AttendeeInfo
    {
        [BsonElement("attendeeId")]
        public string AttendeeId { get; set; }

        [BsonElement("name")]
        public string Name { get; set; }
    }
}
