using MongoDB.Driver;
using NegusEventsApi.Models.Event;
using NegusEventsApi.Models.Feedback;
using NegusEventsApi.Models.Ticket;
using NegusEventsApi.Models.User;

namespace NegusEventsApi.Services
{
    public class IndexManager
    {
        private readonly IMongoDatabase _database;

        public IndexManager(IMongoDatabase database)
        {
            _database = database;
        }

        public void CreateIndexes()
        {
            // Users Collection
            var usersCollection = _database.GetCollection<Users>("User");
            usersCollection.Indexes.CreateOne(new CreateIndexModel<Users>(Builders<Users>.IndexKeys.Ascending(u => u.Email)));
            usersCollection.Indexes.CreateOne(new CreateIndexModel<Users>(Builders<Users>.IndexKeys.Ascending(u => u.Role)));

            // Events Collection
            var eventsCollection = _database.GetCollection<Events>("Event");
            eventsCollection.Indexes.CreateOne(new CreateIndexModel<Events>(Builders<Events>.IndexKeys.Ascending(e => e.Organizer.OrganizerId)));
            eventsCollection.Indexes.CreateOne(new CreateIndexModel<Events>(Builders<Events>.IndexKeys.Ascending(e => e.StartDate)));
            eventsCollection.Indexes.CreateOne(new CreateIndexModel<Events>(Builders<Events>.IndexKeys.Ascending(e => e.EndDate)));
            eventsCollection.Indexes.CreateOne(new CreateIndexModel<Events>(Builders<Events>.IndexKeys.Combine(
                Builders<Events>.IndexKeys.Ascending(e => e.Location.City),
                Builders<Events>.IndexKeys.Ascending(e => e.Location.Country))));
            eventsCollection.Indexes.CreateOne(new CreateIndexModel<Events>(Builders<Events>.IndexKeys.Ascending(e => e.Category)));
            eventsCollection.Indexes.CreateOne(new CreateIndexModel<Events>(Builders<Events>.IndexKeys.Ascending(e => e.Status)));

            // Tickets Collection
            var ticketsCollection = _database.GetCollection<Tickets>("Ticket");
            ticketsCollection.Indexes.CreateOne(new CreateIndexModel<Tickets>(Builders<Tickets>.IndexKeys.Ascending(t => t.Attendee.AttendeeId)));
            ticketsCollection.Indexes.CreateOne(new CreateIndexModel<Tickets>(Builders<Tickets>.IndexKeys.Ascending(t => t.Event.EventId)));
            ticketsCollection.Indexes.CreateOne(new CreateIndexModel<Tickets>(Builders<Tickets>.IndexKeys.Ascending(t => t.TicketType)));

            // Feedback Collection
            var feedbackCollection = _database.GetCollection<Feedbacks>("Feedback");
            feedbackCollection.Indexes.CreateOne(new CreateIndexModel<Feedbacks>(Builders<Feedbacks>.IndexKeys.Ascending(f => f.Event.EventId)));
            feedbackCollection.Indexes.CreateOne(new CreateIndexModel<Feedbacks>(Builders<Feedbacks>.IndexKeys.Ascending(f => f.Attendee.AttendeeId)));
        }
    }

}
