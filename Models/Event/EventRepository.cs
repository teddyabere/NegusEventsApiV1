using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MongoDB.Bson;
using MongoDB.Driver;
using NegusEventsApi.DTO;
using NegusEventsApi.Models.Ticket;
using StackExchange.Redis;

namespace NegusEventsApi.Models.Event
{
    public class EventRepository : IEventRepository
    {
        private readonly IMongoCollection<Events> _events;
        private readonly IDatabase _redisDatabase;

        public EventRepository(IOptions<NegusEventsDbSettings> negusDbSettings, IDatabase redisDatabase)
        {
            var client = new MongoClient(negusDbSettings.Value.ConnectionString);
            var database = client.GetDatabase(negusDbSettings.Value.DatabaseName);
            _events = database.GetCollection<Events>("Event");
            _redisDatabase = redisDatabase;
        }

        public async Task<IEnumerable<Events>> GetAllEventsAsync()
        {
            return await _events.Find(_ => true).SortBy(e => e.StartDate).ToListAsync();
        }
        public async Task<IEnumerable<Events>> GetActiveEventsAsync()
        {
            return await _events.Find(x=> x.Status == EventStatus.Published.ToString() || x.Status == EventStatus.Extended.ToString())
                .SortBy(e => e.StartDate).ToListAsync();
        }
        public async Task<Events> GetEventByIdandUserIdAsync(string id, string userId)
        {
            return await _events.Find(e => e.Id == id && e.Organizer.OrganizerId == userId).FirstOrDefaultAsync();
        }
        public async Task<Events> GetEventByIdAsync(string id)
        {
            return await _events.Find(e => e.Id == id).FirstOrDefaultAsync();
        }

        public async Task<Events> CreateEventAsync(Events eventItem)
        {
            try
            {
                await _events.InsertOneAsync(eventItem);
                string redisKey = $"event:{eventItem.Id}:available_tickets";
                await _redisDatabase.StringSetAsync(redisKey, eventItem.MaximumAttendees);

                return eventItem;
            }
            catch (Exception ex)
            {
                throw new Exception("Error creating the event.", ex);
            }
        }

        public async Task UpdateEventAsync(string id, Events eventItem)
        {
            await _events.ReplaceOneAsync(e => e.Id == id, eventItem);
        }

        public async Task DeleteEventAsync(string id)
        {
            await _events.DeleteOneAsync(e => e.Id == id);
        }
       
        public async Task<List<Events>> SearchEventsAsync(SearchEventsDTO searchCriteria)
        {
            var filterBuilder = Builders<Events>.Filter;
            var filters = new List<FilterDefinition<Events>>();

            if (!string.IsNullOrEmpty(searchCriteria.Name))
            {
                filters.Add(filterBuilder.Regex("Name", new BsonRegularExpression(searchCriteria.Name, "i")));
            }

            if (!string.IsNullOrEmpty(searchCriteria.Description))
            {
                filters.Add(filterBuilder.Regex("Description", new BsonRegularExpression(searchCriteria.Description, "i")));
            }

            if (!string.IsNullOrEmpty(searchCriteria.City))
            {
                filters.Add(filterBuilder.Eq("Location.City", searchCriteria.City));
            }

            if (!string.IsNullOrEmpty(searchCriteria.Country))
            {
                filters.Add(filterBuilder.Eq("Location.Country", searchCriteria.Country));
            }

            if (searchCriteria.MinPrice.HasValue)
            {
                filters.Add(filterBuilder.Gte("Price.Amount", searchCriteria.MinPrice.Value));
            }

            if (searchCriteria.MaxPrice.HasValue)
            {
                filters.Add(filterBuilder.Lte("Price.Amount", searchCriteria.MaxPrice.Value));
            }

            if (searchCriteria.MinAttendees.HasValue)
            {
                filters.Add(filterBuilder.Gte("MaxAttendees", searchCriteria.MinAttendees.Value));
            }

            if (searchCriteria.MaxAttendees.HasValue)
            {
                filters.Add(filterBuilder.Lte("MaxAttendees", searchCriteria.MaxAttendees.Value));
            }

            if (!string.IsNullOrEmpty(searchCriteria.Status))
            {
                filters.Add(filterBuilder.Eq("Status", searchCriteria.Status));
            }

            if (!string.IsNullOrEmpty(searchCriteria.Category))
            {
                filters.Add(filterBuilder.Eq("Category", searchCriteria.Category));
            }

            
            var combinedFilter = filters.Any() ? filterBuilder.And(filters) : FilterDefinition<Events>.Empty;
            Console.WriteLine($"Applied Filter: {combinedFilter}");

            return await _events.Find(combinedFilter).ToListAsync();
        }
        public async Task<List<Events>> GetEventByOrganizerIdAsync(string organizerId)
        {
            return await _events.Find(e => e.Organizer.OrganizerId == organizerId).SortBy(e => e.StartDate).ToListAsync();
        }

        public async Task<List<Events>> GetEventByCityandCountryAsync(string city, string country)
        {
            var eventsInLocation = await _events.Find(e => e.Location.City == city && e.Location.Country == country && e.StartDate != null)
                .SortBy(e => e.StartDate).ToListAsync();

            return eventsInLocation;
        }

        public async Task<List<Events>> GetEventByCountryAsync(string country)
        {
            if (string.IsNullOrWhiteSpace(country))
            {
                throw new ArgumentException("Country cannot be null or empty.", nameof(country));
            }

            var eventsInCountry = await _events
                .Find(e => e.Location != null && e.Location.Country == country && e.StartDate != null) // Filter out documents with null StartDate
                .SortBy(e => e.StartDate)
                .ToListAsync();

            return eventsInCountry;
        }

        public async Task<List<Events>> GetEventByCityAsync(string city)
        {
            if (string.IsNullOrWhiteSpace(city))
            {
                throw new ArgumentException("City cannot be null or empty.", nameof(city));
            }

            var eventsInCity = await _events.Find(e => e.Location.City == city && e.StartDate != null)
                .SortBy(e => e.StartDate)
                .ToListAsync();

            return eventsInCity;
        }

    }
}
