using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;
using NegusEventsApi.Models.Event;

namespace NegusEventsApi.DTO
{
    public class EventDTO
    {
            public string? Id { get; set; }
            public string OrganizerId { get; set; }
            public string Name { get; set; }
            public string Description { get; set; }
            public string StartDate { get; set; }
            public string EndDate { get; set; }
            public Location Location { get; set; }
            public string Category { get; set; }
            public int MaxAttendees { get; set; }
            public string Status { get; set; }
            public string CreatedAt { get; set; }
            public string UpdatedAt { get; set; }
        
    }
}
