using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;
using System;

namespace NegusEventsApi.Models.Ticket
{
    public enum TicketStatus
    {
        Reserved,
        Confirmed,
        Cancelled
    }
    enum PaymentStatus
    {
        Pending, Paid, Failed
    }

    public class Attendee
    {
        [BsonElement("attendeeId")]
        public string AttendeeId { get; set; }
        
        [BsonElement("name")]
        public string Name { get; set; }
    }

    public class Payments
    {
        [BsonElement("amountPaid")]
        public Int32 AmountPaid { get; set; }

        [BsonElement("paymentStatus")]
        public string PaymentStatus { get; set; }

        [BsonElement("paymentMethod")]
        public string PaymentMethod { get; set; }

        [BsonElement("paymentDate")]
        public string PaymentDate { get; set; }
    }

    public class Event
    {
        [BsonElement("eventId")]
        public string EventId { get; set; }
        
        [BsonElement("name")]
        public string Name { get; set; }
        
        [BsonElement("startDate")]
        public string StartDate { get; set; }
        
        [BsonElement("country")]
        public string Country { get; set; }
        
        [BsonElement("organizerId")]
        public string OrganizerId { get; set; }
    }

    public class Tickets
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string? Id { get; set; }
        
        [BsonElement("event")]
        public Event Event { get; set; }
        
        [BsonElement("attendee")]
        public Attendee? Attendee { get; set; }
        
        [BsonElement("ticketType")]
        public string TicketType { get; set; }
        
        [BsonElement("status")]
        [BsonRepresentation(BsonType.String)]
        public TicketStatus? Status { get; set; }
        
        [BsonElement("ticketQuantity")]
        public int TicketQuantity { get; set; }
        
        [BsonElement("isPaid")]
        public bool? IsPaid { get; set; }
        
        [BsonElement("payments")]
        public Payments? Payments { get; set; }
        
        [BsonElement("createdAt")]
        public string CreatedAt { get; set; }
        
        [BsonElement("updatedAt")]
        public string UpdatedAt { get; set; }
    }
}
