using Microsoft.Extensions.Options;
using MongoDB.Bson;
using MongoDB.Driver;

namespace NegusEventsApi.Models.Feedback
{
    public class FeedbackRepository : IFeedbackRepository
    {
        private readonly IMongoCollection<Feedbacks> _feedbackCollection;

        public FeedbackRepository(IOptions<NegusEventsDbSettings> negusDbSettings)
        {
            var client = new MongoClient(negusDbSettings.Value.ConnectionString);
            var database = client.GetDatabase(negusDbSettings.Value.DatabaseName);
            _feedbackCollection = database.GetCollection<Feedbacks>("Feedback");
        }

        public async Task<IEnumerable<Feedbacks>> GetAllFeedbackAsync()
        {
            var result = await _feedbackCollection.Find(_ => true).ToListAsync();
            return result;
        }

        public async Task<Feedbacks> GetFeedbackByIdAsync(string id)
        {
            return await _feedbackCollection.Find(f => f.Id == id).FirstOrDefaultAsync();
        }

        public async Task AddFeedbackAsync(Feedbacks feedback)
        {
            await _feedbackCollection.InsertOneAsync(feedback);
        }

        public async Task UpdateFeedbackAsync(string id, Feedbacks feedback)
        {
            await _feedbackCollection.ReplaceOneAsync(f => f.Id == id, feedback);
        }

        public async Task DeleteFeedbackAsync(string id)
        {
            await _feedbackCollection.DeleteOneAsync(f => f.Id == id);
        }
        public async Task<double> GetAverageRatingAsync(string eventId)
        {
            var pipeline = new[]
            {
        new BsonDocument("$match", new BsonDocument("event.eventId", eventId)),
        new BsonDocument("$group", new BsonDocument
        {
            { "_id", "$event.eventId" },
            { "averageRating", new BsonDocument("$avg", "$rating") }
        })
    };

            var result = await _feedbackCollection.Aggregate<BsonDocument>(pipeline).FirstOrDefaultAsync();
            return result != null ? result["averageRating"].AsDouble : 0.0;
        }

    }
}
