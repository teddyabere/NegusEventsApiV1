using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;

namespace NegusEventsApi.Models.User
{
    public class EventDetail
    {
        [BsonElement("eventId")]
        public string EventId { get; set; }

        [BsonElement("name")]
        public string Name { get; set; }

        [BsonElement("startDate")]
        public DateTime StartDate { get; set; }
    }

    public class Profile
    {
        [BsonElement("phone")]
        public string Phone { get; set; }

        [BsonElement("country")]
        public string Country { get; set; }

        [BsonElement("city")]
        public string City { get; set; }
    }

    public class TicketDetail
    {
        [BsonElement("ticketId")]
        public string TicketId { get; set; }

        [BsonElement("ticketType")]
        public string TicketType { get; set; }

        [BsonElement("amountPaid")]
        public decimal AmountPaid { get; set; }

        [BsonElement("eventName")]
        public string EventName { get; set; }
    }

    public class Users
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string? Id { get; set; }

        [BsonElement("name")]
        public string Name { get; set; }

        [BsonElement("email")]
        public string Email { get; set; }

        [BsonElement("password")]
        public string Password { get; set; }

        [BsonElement("profile")]
        public Profile Profile { get; set; }

        [BsonElement("role")]
        public string Role { get; set; } = "Attendee";

        [BsonElement("status")]
        public string Status { get; set; } = "Approved";

        [BsonElement("events")]
        [BsonIgnoreIfNull]
        public List<EventDetail>? Events { get; set; }

        [BsonElement("tickets")]
        [BsonIgnoreIfNull]
        public List<TicketDetail>? Tickets { get; set; }

        [BsonElement("createdAt")]
        public DateTime CreatedAt { get; set; }

        [BsonElement("updatedAt")]
        public DateTime UpdatedAt { get; set; }
    }

    public class ResetForgotPasswordRequest
    {
        public string Token { get; set; }
        public string Email { get; set; }
        public string NewPassword { get; set; }
    }

}
