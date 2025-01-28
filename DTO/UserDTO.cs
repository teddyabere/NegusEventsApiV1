using MongoDB.Bson.Serialization.Attributes;

namespace NegusEventsApi.DTO
{
    public class UserDTO
    {
        public string Id { get; set; } 
        public string Name { get; set; } 
        public string Email { get; set; }
        public string Role { get; set; } 
        public string Status { get; set; }
    }

}
