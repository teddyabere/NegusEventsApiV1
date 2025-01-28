using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;
using NegusEventsApi.Models.Ticket;
using System.Text.Json.Nodes;

namespace NegusEventsApi.DTO
{
    public class AttendeeDTO
    {
        public string AttendeeId { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
    }

    public class TicketDTO
    {
        public string Id { get; set; }
        public string EventId { get; set; } 
        public AttendeeDTO Attendee { get; set; }
        public string TicketType { get; set; }
        public string PurchaseDate { get; set; }
        public string Status { get; set; }
    }

}
