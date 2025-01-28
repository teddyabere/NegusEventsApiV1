using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Options;
using MongoDB.Bson.IO;
using MongoDB.Driver;
using NegusEventsApi.Models.Event;
using NegusEventsApi.Models.Ticket;
using StackExchange.Redis;
using System.Text.Json;
using Newtonsoft.Json;

using static System.Reflection.Metadata.BlobBuilder;
using JsonSerializer = System.Text.Json.JsonSerializer;
using JsonConvert = Newtonsoft.Json.JsonConvert;


namespace NegusEventsApi.Redis
{
    public class RedisCacheService : IRedisCacheService
    {
        private readonly IConnectionMultiplexer _redisConnection;
        private readonly IDatabase _redisDatabase;
        private readonly IMongoCollection<Events> _eventCollection;

        public RedisCacheService(IConnectionMultiplexer redisConnection)
        {
            _redisConnection = redisConnection;
            _redisDatabase = _redisConnection.GetDatabase();
        }

        public async Task SetCacheAsync<T>(string key, T value, TimeSpan expiration)
        {
            var jsonData = System.Text.Json.JsonSerializer.Serialize(value);
            await _redisDatabase.StringSetAsync(key, jsonData, expiration);
        }

        public async Task<T?> GetCacheAsync<T>(string key)
        {
            var data = await _redisDatabase.StringGetAsync(key);
            if (data.IsNullOrEmpty)
            {
                return default;
            }
            return JsonSerializer.Deserialize<T>(data);
        }

        public async Task RemoveCacheAsync(string key)
        {
            await _redisDatabase.KeyDeleteAsync(key);
        }

        public async Task CacheAttendeeEventsAsync(string attendeeId, string eventId)
        {
            var key = $"registered:attendee:{attendeeId}";
            await _redisDatabase.KeyDeleteAsync(key);
            await _redisDatabase.SetAddAsync(key, eventId);
        }

        public async Task<List<string>> GetAttendeeEventsAsync(string attendeeId)
        {
            var key = $"registered:attendee:{attendeeId}";
            var events = await _redisDatabase.SetMembersAsync(key);
            return events.Select(x => x.ToString()).ToList();
        }
        
        public async Task InitializeAvailableTicketsAsync(string eventId, int maxAttendees)
        {
            string redisKey = $"event:{eventId}:available_tickets";
            await _redisDatabase.StringSetAsync(redisKey, maxAttendees);
        }

        public async Task<int> GetAvailableTicketsAsync(string eventId)
        {
            string redisKey = $"event:{eventId}:available_tickets";
            return (int)(await _redisDatabase.StringGetAsync(redisKey));
        }

        public async Task<bool> ValidateAvailableTicketsAsync(string eventId)
        {
            var eventDetails = await _eventCollection.Find(e => e.Id == eventId).FirstOrDefaultAsync();
            if (eventDetails == null) throw new Exception("Event not found.");

            int availableTickets = await GetAvailableTicketsAsync(eventId);
            return availableTickets <= eventDetails.MaximumAttendees;
        }

        public async Task<bool> HasUserPurchasedTicketAsync(string userId, string eventId)
        {
            string redisKey = $"user:{userId}:event:{eventId}";
            return await _redisDatabase.KeyExistsAsync(redisKey);
        }

        public async Task MarkUserTicketPurchasedAsync(string userId, string eventId)
        {
            string redisKey = $"user:{userId}:event:{eventId}";
            await _redisDatabase.StringSetAsync(redisKey, true);

            string userReservationsKey = $"confirmed:{userId}";
            await _redisDatabase.ListRightPushAsync(userReservationsKey, redisKey);
            await CacheAttendeeEventsAsync(userId, eventId);

        }
        public async Task<int> DecrementAvailableTicketsAsync(string eventId, int numberOfTickets)
        {
            if (numberOfTickets <= 0)
            {
                throw new ArgumentException("Number of tickets to decrement must be greater than zero.", nameof(numberOfTickets));
            }

            string redisKey = $"event:{eventId}:available_tickets";
            var availableTickets = await _redisDatabase.StringGetAsync(redisKey);

            if (!availableTickets.HasValue || (int)availableTickets <= 0)
            {
                throw new InvalidOperationException("Tickets are Sold Out.");
            }

            if (!availableTickets.HasValue || (int)availableTickets < numberOfTickets)
            {
                throw new InvalidOperationException("Not enough tickets available.");
            }

            var decrementedNum = await _redisDatabase.StringDecrementAsync(redisKey, numberOfTickets);

            return (int)decrementedNum;
        }

        public async Task IncrementAvailableTicketsAsync(string eventId)
        {
            string redisKey = $"event:{eventId}:available_tickets";
            await _redisDatabase.StringIncrementAsync(redisKey);
        }

        public async Task ReserveTicketAsync(string userId, string eventId)
        {
            string reservationKey = $"reservation:user:{userId}:event:{eventId}";

            if (await _redisDatabase.KeyExistsAsync(reservationKey))
                throw new Exception("User already has a pending reservation for this event.");

            await _redisDatabase.StringSetAsync(reservationKey, true, TimeSpan.FromMinutes(15));

            string userReservationsKey = $"reservations:{userId}";
            await _redisDatabase.ListRightPushAsync(userReservationsKey, reservationKey);

            string eventReservationsKey = $"reservations:event:{eventId}";
            await _redisDatabase.ListRightPushAsync(eventReservationsKey, userId);
        }


        public async Task<bool> CheckReservationAsync(string userId, string eventId)
        {
            string reservationKey = $"reservation:user:{userId}:event:{eventId}";

            
            if (!await _redisDatabase.KeyExistsAsync(reservationKey))
                return false;

            return true;
        }

        public async Task<List<Tickets>> GetValidReservedTicketsAsync(string attendeeId)
        {
            var redisKeysPattern = $"reservations:{attendeeId}:*";
            var reservedTickets = new List<Tickets>();

            var server = _redisConnection.GetServer(_redisConnection.GetEndPoints().First());
            var keys = server.Keys(pattern: redisKeysPattern);

            foreach (var key in keys)
            {
                var ticketDetails = await _redisDatabase.StringGetAsync(key);

                if (!ticketDetails.IsNullOrEmpty)
                {
                    var ticket = JsonConvert.DeserializeObject<Tickets>(ticketDetails);
                    reservedTickets.Add(ticket);
                }
            }

            return reservedTickets;
        }

        public async Task<List<string>> GetConfirmedTickets1Async(string userId)
        {
            string userReservationsKey = $"confirmed:{userId}";
            
            var confirmedTickets = await _redisDatabase.ListRangeAsync(userReservationsKey);

            return confirmedTickets.Select(ticket => (string)ticket).ToList();
        }

        public async Task<bool> CleanupExpiredReservationsAsync(string attendeeId)
        {
            string userReservationsKey = $"reservations:{attendeeId}";
            var keys = await _redisDatabase.ListRangeAsync(userReservationsKey);

            foreach (var key in keys)
            {
                bool exists = await _redisDatabase.KeyExistsAsync(key.ToString());
                if (!exists)
                {
                    await _redisDatabase.ListRemoveAsync(userReservationsKey, key);
                }
            }
            return true;
        }
        public async Task<List<string>> GetReservationsByEventIdAsync(string eventId)
        {
            string eventReservationsKey = $"reservations:event:{eventId}";

            var userIds = await _redisDatabase.ListRangeAsync(eventReservationsKey);

            return userIds.Select(x => x.ToString()).ToList();
        }


        public async Task CancelReservationAsync(string userId, string eventId)
        {
            string reservationKey = $"reservation:user:{userId}:event:{eventId}";
            string ticketsKey = $"event:{eventId}:available_tickets";

            if (!await _redisDatabase.KeyExistsAsync(reservationKey))
                throw new Exception("Reservation not found or expired.");

            
            await _redisDatabase.StringIncrementAsync(ticketsKey);

            await _redisDatabase.KeyDeleteAsync(reservationKey);
        }
       
    }
}
