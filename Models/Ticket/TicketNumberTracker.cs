using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace NegusEventsApi.Models.Ticket
{
    public class TicketNumberTracker
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string? Id { get; set; }
        public string EventId { get; set; }
        public string EventCategory { get; set; }
        public string CatagoryShortName { get; set; }
        public int Year { get; set; }
        public int LastNumber { get; set; }

        public TicketNumberTracker(string eventId, string category, string catN, int startY, int lastNumber)
        {
            this.EventId = eventId;
            this.EventCategory = category;
            this.CatagoryShortName = catN;
            this.Year = startY;
            this.LastNumber = lastNumber;
        }
        public void Increment()
        {
            LastNumber++;
        }
    }
}
