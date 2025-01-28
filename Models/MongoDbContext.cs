using Microsoft.Extensions.Options;
using MongoDB.Driver;
using NegusEventsApi.Models.Event;
using NegusEventsApi.Models.Feedback;
using NegusEventsApi.Models.Ticket;
using NegusEventsApi.Models.User;

namespace NegusEventsApi.Models
{
    public class MongoDbContext
    {
        private readonly IMongoDatabase _database;

        public MongoDbContext(IOptions<NegusEventsDbSettings> negusDbSettings)
        {
            var client = new MongoClient(negusDbSettings.Value.ConnectionString);
             _database = client.GetDatabase(negusDbSettings.Value.DatabaseName);
            //var client = new MongoClient(connectionString);
            //_database = client.GetDatabase(databaseName);
        }

        public IMongoCollection<Tickets> Tickets => _database.GetCollection<Tickets>("Ticket");
        public IMongoCollection<Events> Events => _database.GetCollection<Events>("Event");
        public IMongoCollection<Users> Users => _database.GetCollection<Users>("User");
        public IMongoCollection<Feedbacks> Feedbacks => _database.GetCollection<Feedbacks>("Feedback");

    }
}
