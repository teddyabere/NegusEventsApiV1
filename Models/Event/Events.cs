using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;

namespace NegusEventsApi.Models.Event
{
    enum EventStatus
    {
        Pending,
        Published,
        Started,
        Extended,
        Cancelled,
        Ended
    }

    public class TicketType
    {
        [BsonElement("type")]
        public string Type { get; set; }

        [BsonElement("price")]
        public Int32 Price { get; set; }
    }

    public class Location
    {
        [BsonElement("address")]
        public string Address { get; set; }

        [BsonElement("city")]
        public string City { get; set; }

        [BsonElement("state")]
        public string State { get; set; }

        [BsonElement("zipCode")]
        public string ZipCode { get; set; }

        [BsonElement("country")]
        public string Country { get; set; }
    }


    public class OrganizerInfo
    {
        [BsonElement("organizerId")]
        public string OrganizerId { get; set; }

        [BsonElement("name")]
        public string Name { get; set; }
    }

    public class Events
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string? Id { get; set; }

        [BsonElement("organizer")]
        public OrganizerInfo? Organizer { get; set; }

        [BsonElement("name")]
        public string Name { get; set; }

        [BsonElement("description")]
        public string Description { get; set; }

        [BsonElement("startDate")]
        public DateTime StartDate { get; set; }

        [BsonElement("endDate")]
        public DateTime EndDate { get; set; }

        [BsonElement("location")]
        public Location Location { get; set; }

        [BsonElement("category")]
        public string Category { get; set; }

        [BsonElement("maximumAttendees")]
        public int MaximumAttendees { get; set; }

        [BsonElement("quantityAvailable")]
        public int? QuantityAvailable { get; set; }

        [BsonElement("quantitySold")]
        public int? QuantitySold { get; set; }

        [BsonElement("ticketTypes")]
        public TicketType TicketTypes { get; set; }

        [BsonElement("status")]
        public string? Status { get; set; }

        [BsonElement("createdAt")]
        public DateTime CreatedAt { get; set; }

        [BsonElement("updatedAt")]
        public DateTime UpdatedAt { get; set; }
    }
}
