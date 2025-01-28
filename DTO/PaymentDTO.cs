using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;

namespace NegusEventsApi.DTO
{
    public class PaymentDTO
    {
        public string Id { get; set; }

        public string TicketId { get; set; }

        public string EventId { get; set; }

        public string AttendeeId { get; set; }

        public decimal AmountPaid { get; set; }

        public string PaymentStatus { get; set; }

        public string PaymentMethod { get; set; }
    }
}
